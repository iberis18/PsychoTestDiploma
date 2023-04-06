using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DAL.Interfaces;
using DAL.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json.Linq;

namespace DAL.Repositories
{
    public class TestRepository : ITestRepository<Test>
    {
        private Context db;

        public TestRepository(Context dbcontext)
        {
            this.db = dbcontext;
        }


        //получаем краткий список всех тестов в формате id-название-заголовок-инструкция
        public async Task<IEnumerable<Test>> GetAll()
        {
            return (await db.TestsBson.Find(new BsonDocument()).ToListAsync())
                .Select(i => new Test()
                {
                    name = i["IR"]["Name"]["#text"].AsString,
                    id = i["_id"].AsObjectId.ToString(),
                    title = i["IR"]["Title"]["#text"].AsString,
                    instruction = i["Instruction"]["#text"].AsString
                });
        }

        //получаем тест по id
        public async Task<string> GetItemById(string id)
        {
            var bsonDoc = await db.TestsBson.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
            var dotNetObj = BsonTypeMapper.MapToDotNetValue(bsonDoc);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dotNetObj);
            var jObj = JObject.Parse(json);
            if (jObj["Questions"] != null && jObj["Questions"].ToString() != "")
                foreach (var question in jObj["Questions"]["item"])
                {
                    if (question["Question_Type"].ToString() == "1" && question["ImageFileName"] != null)
                    {
                        string imageName = question["ImageFileName"].ToString();
                        byte[] image = await db.GridFs.DownloadAsBytesByNameAsync(imageName);
                        question["Image"] = "data:image/" + Path.GetExtension(imageName).Replace(".", "") + ";base64," + Convert.ToBase64String(image);
                    }
                    foreach (var answer in question["Answers"]["item"])
                        if (answer["Answer_Type"].ToString() == "2")
                        {
                            string imageName = answer["ImageFileName"].ToString();
                            byte[] image = await db.GridFs.DownloadAsBytesByNameAsync(imageName);
                            answer["Image"] = "data:image/" + Path.GetExtension(imageName).Replace(".", "") + ";base64," + Convert.ToBase64String(image);
                        }
                }
            return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
        }

        //получаем тест по id
        public async Task<string> GetItemByIdWithoutImages(string id)
        {
            var bsonDoc = await db.TestsBson.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
            var dotNetObj = BsonTypeMapper.MapToDotNetValue(bsonDoc);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dotNetObj);
            return json;
        }

        public async Task<BsonDocument> GetBsonItemByIRId(string id)
        {
            //return await db.TestsBson.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
            return await db.TestsBson.Find(new BsonDocument("IR.ID", id)).FirstOrDefaultAsync();
        }

        public async Task<BsonDocument> GetBsonItemById(string id)
        {
            return await db.TestsBson.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }


        //получаем все назначенные пациенту тесты 
        //public async Task<IEnumerable<Test>> GetTestsByPatientToken(Patient patient)
        //{
        //    List<Test> tests = new List<Test>();
        //    var documents = await db.TestsBson.Find(new BsonDocument()).ToListAsync();
        //    if (patient.tests != null)
        //    {
        //        foreach (string idTest in patient.tests)
        //        {
        //            foreach (BsonDocument doc in documents)
        //            {
        //                if (idTest == doc["_id"].AsObjectId.ToString())
        //                {
        //                    tests.Add(new Test
        //                    {
        //                        name = doc["IR"]["name"]["#text"].AsString,
        //                        id = doc["_id"].AsObjectId.ToString(),
        //                        title = doc["IR"]["title"]["#text"].AsString,
        //                        instruction = doc["instruction"]["#text"].AsString
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    return tests;
        //}

        //Импорт теста
        public async Task ImportTestFile(string file)
        {
            await db.TestsBson.InsertOneAsync(BsonDocument.Parse(file));
        }

        //Импорт норм
        public async Task ImportNormFile(string file)
        {
            await db.NormsBson.InsertOneAsync(BsonDocument.Parse(file));
        }

        //получаем список id всех норм
        public async Task<IEnumerable<string>> GetNorms()
        {
            var documents = await db.NormsBson.Find(new BsonDocument()).ToListAsync();
            List<string> norms = new List<string>();
            foreach (BsonDocument doc in documents)
            {
                norms.Add(doc["_id"].AsObjectId.ToString());
            }
            return norms;
        }

        public async Task<BsonDocument> GetNormByIRId(string id)
        {
            return await db.NormsBson.Find(new BsonDocument("main.groups.item.id", id)).FirstOrDefaultAsync();
        }

        // удаление теста вместе с нормой и изображениями
        public async Task Remove(string id)
        {
            var test = await db.TestsBson.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
            //удаляем норму
            await db.NormsBson.DeleteOneAsync(new BsonDocument("main.groups.item.id", test["IR"]["ID"].AsString));
            //удаляем тест
            await db.Tests.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));

            //удаляем изображения
            var dotNetObj = BsonTypeMapper.MapToDotNetValue(test);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dotNetObj);
            var jObj = JObject.Parse(json);
            foreach (var question in jObj["Questions"]["item"])
            {
                if (question["Question_Type"].ToString() == "1" && question["ImageFileName"] != null)
                {
                    var builder = new FilterDefinitionBuilder<GridFSFileInfo>();
                    var filter = Builders<GridFSFileInfo>.Filter.Eq<string>(info => info.Filename, question["ImageFileName"].ToString());
                    var fileInfo = await db.GridFs.Find(filter).FirstOrDefaultAsync();
                    if (fileInfo != null)
                        await db.GridFs.DeleteAsync(fileInfo.Id);
                }
                foreach (var answer in question["Answers"]["item"])
                    if (answer["Answer_Type"].ToString() == "2")
                    {
                        var builder = new FilterDefinitionBuilder<GridFSFileInfo>();
                        var filter = Builders<GridFSFileInfo>.Filter.Eq<string>(info => info.Filename, answer["ImageFileName"].ToString());
                        var fileInfo = await db.GridFs.Find(filter).FirstOrDefaultAsync();
                        if (fileInfo != null)
                            await db.GridFs.DeleteAsync(fileInfo.Id);
                    }
            }
        }

        // сохранение изображения
        public async Task ImportImage(Stream imageStream, string imageName)
        {
            await db.GridFs.UploadFromStreamAsync(imageName, imageStream);
        }

    }
}