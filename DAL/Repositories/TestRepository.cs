using System;
using System.Collections.Generic;
using System.IO;
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
            var documents = await db.TestsBson.Find(new BsonDocument()).ToListAsync();
            List<Test> tests = new List<Test>();
            foreach (BsonDocument doc in documents)
            {
                tests.Add(new Test
                {
                    name = doc["IR"]["name"]["#text"].AsString,
                    id = doc["_id"].AsObjectId.ToString(),
                    title = doc["IR"]["title"]["#text"].AsString,
                    instruction = doc["instruction"]["#text"].AsString
                });
            }
            return tests;
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

        //получаем все назначенные пациенту тесты 
        public async Task<IEnumerable<Test>> GetTestsByPatientToken(Patient patient)
        {
            List<Test> tests = new List<Test>();
            var documents = await db.TestsBson.Find(new BsonDocument()).ToListAsync();
            if (patient.tests != null)
            {
                foreach (string idTest in patient.tests)
                {
                    foreach (BsonDocument doc in documents)
                    {
                        if (idTest == doc["_id"].AsObjectId.ToString())
                        {
                            tests.Add(new Test
                            {
                                name = doc["IR"]["name"]["#text"].AsString,
                                id = doc["_id"].AsObjectId.ToString(),
                                title = doc["IR"]["title"]["#text"].AsString,
                                instruction = doc["instruction"]["#text"].AsString
                            });
                        }
                    }
                }
            }
            return tests;
        }

        //Импорт теста
        public async Task<string> ImportTestFile(string file)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(file);
            var json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.None, true);
            var jObj = JObject.Parse(json);

            var test = await db.TestsBson.Find(new BsonDocument("IR.ID", jObj["IR"]["ID"].ToString())).FirstOrDefaultAsync();
            if (test == null)
            {
                int i = 0;
                if (jObj["Questions"] != null && jObj["Questions"].ToString() != "")
                    foreach (var question in jObj["Questions"]["item"])
                    {
                        question["Question_id"] = i;
                        i++;

                        //если вопрос только с текстом — Question_Type = 0, если с изображением Question_Type = 1
                        if (question["ImageFileName"] != null)
                            question["Question_Type"] = 1;
                        else
                            question["Question_Type"] = 0;

                        //если вопрос c выбором одного отета — Question_Choice = 1, если с вводом своего — Question_Choice = 0
                        if (question["TypeQPanel"] != null)
                        {
                            if (question["TypeQPanel"].ToString() == "2" || question["AnsString_Num"] != null ||
                                        question["AnsString_ExanineAnsMethod"] != null || question["Ans_CheckUpperCase"] != null)
                            {
                                question["Question_Choice"] = 0;
                                if (question["Answers"]["item"] is JArray)
                                    foreach (var answer in question["Answers"]["item"])
                                        answer["Answer_Type"] = 1;
                                else
                                {
                                    JArray arr = new JArray();
                                    question["Answers"]["item"]["Answer_Type"] = 1;
                                    arr.Add(question["Answers"]["item"]);
                                    question["Answers"]["item"] = arr;
                                }

                            }
                            else
                            {
                                question["Question_Choice"] = 1;
                                if (question["Answers"]["item"] is JArray)
                                    foreach (var answer in question["Answers"]["item"])
                                        answer["Answer_Type"] = 0;
                                else
                                {
                                    JArray arr = new JArray();
                                    question["Answers"]["item"]["Answer_Type"] = 0;
                                    arr.Add(question["Answers"]["item"]);
                                    question["Answers"]["item"] = arr;
                                }
                            }
                        }
                        else
                        {
                            question["Question_Choice"] = 1;
                            if (question["Answers"]["item"] is JArray)
                                foreach (var answer in question["Answers"]["item"])
                                    answer["Answer_Type"] = 0;
                            else
                            {
                                JArray arr = new JArray();
                                question["Answers"]["item"]["Answer_Type"] = 0;
                                arr.Add(question["Answers"]["item"]);
                                question["Answers"]["item"] = arr;
                            }
                        }
                        int j = 0;
                        if (question["Answers"]["item"] is JArray)
                            foreach (var answer in question["Answers"]["item"])
                            {
                                answer["Answer_id"] = j;
                                j++;
                                //если ответ это текст — Answer_Type = 0, если это поле для ввода — Answer_Type = 1, если изображение — Answer_Type = 2;
                                if (answer["ImageFileName"] != null)
                                    answer["Answer_Type"] = 2;
                            }
                        else
                        {
                            JArray arr = new JArray();
                            question["Answers"]["item"]["Answer_id"] = 0;
                            if (question["Answers"]["item"]["ImageFileName"] != null)
                                question["Answers"]["item"]["Answer_Type"] = 2;
                            arr.Add(question["Answers"]["item"]);
                            question["Answers"]["item"] = arr;
                        }
                    }
                var document = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(jObj));
                await db.TestsBson.InsertOneAsync(document);
                return jObj["IR"]["ID"].ToString();
            }
            else return null;
        }

        //Импорт норм
        public async Task ImportNormFile(string file, string testId)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(file);
            var json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc);
            var jObj = JObject.Parse(json);
            jObj["main"]["groups"]["item"]["id"] = testId;
            var document = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(jObj));
            await db.NormsBson.InsertOneAsync(document);
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