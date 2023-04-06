using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BLL.Operations
{
    public class TestOperations : ITest
    {
        IUnitOfWork db; //репозиторий

        public TestOperations(IUnitOfWork db)
        {
            this.db = db;
        }



        //получаем краткий список всех тестов в формате id-название-заголовок-инструкция
        public async Task<IEnumerable<Test>> GetTestsView()
        {
            return (await db.Tests.GetAll()).Select(i => new Test(i));
        }

        //получаем тест по id
        public async Task<string> GetTestById(string id)
        {
            return await db.Tests.GetItemById(id);
        }

        //получаем тест по id
        //public async Task<string> GetTestByIdWithoutImages(string id)
        //{
        //    return await db.Tests.GetItemByIdWithoutImages(id);
        //}

        //Импорт теста
        public async Task<string> ImportTestFile(string file)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(file);
            var jObj = JObject.Parse(JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.None, true));
            var test = await db.Tests.GetBsonItemByIRId(jObj["IR"]["ID"].ToString());
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
                await db.Tests.ImportTestFile(Newtonsoft.Json.JsonConvert.SerializeObject(jObj));
                return jObj["IR"]["ID"].ToString();
            }
            else return null;
        }

        //Импорт норм
        public async Task ImportNormFile(string file, string testId)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(file);
            var jObj = JObject.Parse(JsonConvert.SerializeXmlNode(xmlDoc));
            jObj["main"]["groups"]["item"]["id"] = testId;
            await db.Tests.ImportNormFile(Newtonsoft.Json.JsonConvert.SerializeObject(jObj));
        }

        ////получаем список id всех норм
        //public async Task<IEnumerable<string>> GetNorms()
        //{
        //    var documents = await NormsBson.Find(new BsonDocument()).ToListAsync();
        //    List<string> norms = new List<string>();
        //    foreach (BsonDocument doc in documents)
        //    {
        //        norms.Add(doc["_id"].AsObjectId.ToString());
        //    }
        //    return norms;
        //}

        // удаление теста вместе с нормой и изображениями
        public async Task RemoveTest(string id)
        {
            await db.Tests.Remove(id);
        }

        // сохранение изображения
        public async Task ImportImage(Stream imageStream, string imageName)
        {
            await db.Tests.ImportImage(imageStream, imageName);
        }
    }
}