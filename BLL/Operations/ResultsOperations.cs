using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BLL.Operations
{
    public class ResultsOperations
    {
        IUnitOfWork db; //репозиторий
        private IPatient patientDB;

        public ResultsOperations(IUnitOfWork db)
        {
            this.db = db;
            this.patientDB = new PatientOperations(db);
        }

        //обработка полученных результатов теста
        public async Task Processing(TestsResult result, Patient patient)
        {
            var doc = await db.Tests.GetBsonItemById(result.Id);
            if (doc != null)
            {
                var dotNetObj = BsonTypeMapper.MapToDotNetValue(doc);
                var json = JsonConvert.SerializeObject(dotNetObj);

                //пройденный тест
                var test = JObject.Parse(json);
                //норма для данного теста
                var norm = await db.Tests.GetNormByIRId(test["IR"]["ID"].ToString());

                //обработка результатов

                //обработка люшера
                if (test["IR"]["ClassName"] != null)
                {
                    if (test["IR"]["ClassName"].ToString() == "Lusher")
                    {
                        ProcessingLusherResults processingResults = new ProcessingLusherResults(test, result, norm);
                        DateTime now = DateTime.Now;
                        processingResults.patientResult.Date = now.ToString("g");
                        processingResults.patientResult.Comment = "";
                        processingResults.patientResult.Test = result.Id;
                        //добавление в бд
                        patient.Results.Add(processingResults.patientResult);
                        await patientDB.UpdatePatient(patient);
                    }
                }
                //обработка стандартных опросников
                else
                {
                    ProcessingResults processingResults = new ProcessingResults(test, result, norm);
                    DateTime now = DateTime.Now;
                    processingResults.patientResult.Date = now.ToString("g");
                    processingResults.patientResult.Comment = "";
                    processingResults.patientResult.Test = result.Id;
                    //добавление в бд
                    patient.Results.Add(processingResults.patientResult);
                    await patientDB.UpdatePatient(patient); 
                }
            }
        }
    }

    public class ProcessingResults
    {
        public PatientResult patientResult = new PatientResult();

        public ProcessingResults(JObject test, TestsResult result, BsonDocument norm)
        {
            //подсчет баллов по шкалам
            Dictionary<string, double> sum = Scorting(test, result, patientResult.Scales);

            //добавление шкал пациенту
            AddScales(sum, test);

            //автоматическая интерпретация результатов
            //нахождение диапазонов
            RangeInterpretation(norm, patientResult.Scales);
            //рассчет по формулам
            CalculationByFormulas(test, patientResult.Scales, norm);
        }


        //подсчет баллов по шкалам
        private Dictionary<string, double> Scorting(JObject test, TestsResult result, List<PatientResult.Scale> patientScales)
        {
            //id шкалы - сумма по шкале 
            Dictionary<string, double> scales = new Dictionary<string, double>();
            foreach (var answer in result.Answers)
            {
                //Если вопрос с выбором одного из вариантов ответа
                if (Int32.Parse(test["Questions"]["item"][answer.QuestionId]["Question_Choice"].ToString()) == 1)
                {
                    if (answer.ChosenAnswer != "")
                        if (test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"] != null)
                            if (test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"].ToString() != "")
                            {
                                //id шкалы
                                string scale = "";
                                if (test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"]["item"] is JArray)
                                    foreach (var s in test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"]["item"])
                                    {
                                        scale = s["ScID"].ToString();
                                        if (scales.ContainsKey(scale))
                                            scales[scale] += double.Parse(s["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        else
                                        {
                                            scales[scale] = double.Parse(s["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                    }
                                else
                                {
                                    scale = test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"]["item"]["ScID"].ToString();
                                    if (scales.ContainsKey(scale))
                                        scales[scale] += double.Parse(test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"]["item"]["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    else
                                    {
                                        scales[scale] = double.Parse(test["Questions"]["item"][answer.QuestionId]["Answers"]["item"][Int32.Parse(answer.ChosenAnswer)]["Weights"]["item"]["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                }
                //Если вопрос с вводом своего ответа
                else foreach (var ans in test["Questions"]["item"][answer.QuestionId]["Answers"]["item"])
                {
                    if (answer.ChosenAnswer == ans["Name"]["#text"].ToString())
                        if (ans["Weights"] != null)
                        {
                            if (ans["Weights"]["item"] is JArray)
                            {
                                foreach (var i in ans["Weights"]["item"])
                                    if (scales.ContainsKey(i["ScID"].ToString()))
                                        scales[i["ScID"].ToString()] += double.Parse(i["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    else
                                    {
                                        scales[i["ScID"].ToString()] = double.Parse(i["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                            }
                            else
                            {
                                if (scales.ContainsKey(ans["Weights"]["item"]["ScID"].ToString()))
                                    scales[ans["Weights"]["item"]["ScID"].ToString()] += double.Parse(ans["Weights"]["item"]["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                else
                                {
                                    scales[ans["Weights"]["item"]["ScID"].ToString()] = double.Parse(ans["Weights"]["item"]["Weights"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }
                        }
                }
            }
            return scales;
        }


        //добавление шкал пациенту
        private void AddScales(Dictionary<string, double> sum, JObject test)
        {
            if (test["Groups"]["item"] is JArray)
                foreach (var scale in test["Groups"]["item"])
                    if (sum.ContainsKey(scale["ID"].ToString()))
                        if (scale["NormID"] != null)
                            patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = scale["ID"].ToString(), IdNormScale = scale["NormID"].ToString(), Name = scale["Name"]["#text"].ToString(), Scores = sum[scale["ID"].ToString()] });
                        else
                            patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = scale["ID"].ToString(), Name = scale["Name"]["#text"].ToString(), Scores = sum[scale["ID"].ToString()] });
                    else
                        if (scale["NormID"] != null)
                        patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = scale["ID"].ToString(), IdNormScale = scale["NormID"].ToString(), Name = scale["Name"]["#text"].ToString() });
                    else
                        patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = scale["ID"].ToString(), Name = scale["Name"]["#text"].ToString() });
            else
            if (sum.ContainsKey(test["Groups"]["item"]["ID"].ToString()))
                if (test["Groups"]["item"]["NormID"] != null)
                    patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = test["Groups"]["item"]["ID"].ToString(), IdNormScale = test["Groups"]["item"]["NormID"].ToString(), Name = test["Groups"]["item"]["Name"]["#text"].ToString(), Scores = sum[test["Groups"]["item"]["ID"].ToString()] });
                else
                    patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = test["Groups"]["item"]["ID"].ToString(), Name = test["Groups"]["item"]["Name"]["#text"].ToString(), Scores = sum[test["Groups"]["item"]["ID"].ToString()] });
            else
                if (test["Groups"]["item"]["NormID"] != null)
                patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = test["Groups"]["item"]["ID"].ToString(), IdNormScale = test["Groups"]["item"]["NormID"].ToString(), Name = test["Groups"]["item"]["Name"]["#text"].ToString() });
            else
                patientResult.Scales.Add(new PatientResult.Scale() { IdTestScale = test["Groups"]["item"]["ID"].ToString(), Name = test["Groups"]["item"]["Name"]["#text"].ToString() });
        }


        //автоматическая интерпретация результатов
        private void RangeInterpretation(BsonDocument norm, List<PatientResult.Scale> results)
        {
            //шкалы из норм
            BsonArray normScales = new BsonArray();
            if (norm["main"]["groups"]["item"]["quantities"]["item"] is BsonArray)
                foreach (var scale in norm["main"]["groups"]["item"]["quantities"]["item"].AsBsonArray)
                    normScales.Add(scale);
            else
                normScales.Add(norm["main"]["groups"]["item"]["quantities"]["item"]);


            //Для каждой вычисленной шкалы
            foreach (var result in results)
            {
                //если градация еще не определена
                if (result.GradationNumber == null)
                    foreach (var normScale in normScales)
                    {
                        //находим соответствующую шкалу из норм 
                        if (result.Scores != null && result.IdNormScale == normScale["id"].AsString)
                        {
                            //выбираем все градации шкалы
                            BsonArray grads = new BsonArray();
                            if (normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"] is BsonArray)
                                foreach (var grad in normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"].AsBsonArray)
                                    grads.Add(grad);
                            else
                                grads.Add(normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"]);



                            //Для каждой градации
                            foreach (var bsonGrad in grads)
                            {
                                var grad = JObject.Parse(JsonConvert.SerializeObject(BsonTypeMapper.MapToDotNetValue(bsonGrad)));

                                //если обе границы inf
                                if (grad["lowerformula"]["ftext"].ToString() == "-inf" && grad["upperformula"]["ftext"].ToString() == "+inf")
                                {
                                    if (grad["comment"]["#text"] != null)
                                        result.Interpretation = grad["comment"]["#text"].ToString();
                                    result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                }
                                else
                                //если слева -inf, справа число
                                if (grad["lowerformula"]["ftext"].ToString() == "-inf" && grad["upperformula"]["ftext"].ToString() != "+inf")
                                {
                                    if (result.Scores <= double.Parse(grad["upperformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                                else
                                //если справа +inf, слева число
                                if (grad["lowerformula"]["ftext"].ToString() != "-inf" && grad["upperformula"]["ftext"].ToString() == "+inf")
                                {
                                    if (result.Scores > double.Parse(grad["lowerformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                                else
                                //c обеих сторон числа
                                if (grad["lowerformula"]["ftext"].ToString() != "-inf" && grad["upperformula"]["ftext"].ToString() != "+inf")
                                {
                                    if (result.Scores > double.Parse(grad["lowerformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture) &&
                                        result.Scores <= double.Parse(grad["upperformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                            }
                            break;
                        }
                    }
            }
        }

        private void CalculationByFormulas(JObject test, List<PatientResult.Scale> results, BsonDocument norm)
        {
            JArray scales = new JArray();
            if (test["Groups"]["item"] is JArray)
                foreach (var scale in test["Groups"]["item"])
                    scales.Add(scale);
            else
                scales.Add(test["Groups"]["item"]);

            //для каждой шкалы в результатах
            foreach (var result in results)
                //если баллов нет, будем вычислять
                if (result.Scores == null)
                {
                    foreach (var scale in scales)
                        if (scale["ID"].ToString() == result.IdTestScale)

                        {
                            if (scale["Formula"].ToString() == "ВЕСОВЫЕ КОЭФФИЦИЕНТЫ")
                                result.Scores = 0;
                            else
                            {
                                string formula = scale["Formula"].ToString();
                                //парсим формулу
                                formula = ParseFormula.Parse(formula, results);

                                //расчет баллов по формуле
                                result.Scores = Calculator.Calculate(formula);
                                //повторное нахождение диапазонов
                                RangeInterpretation(norm, patientResult.Scales);
                            }
                            break;
                        }
                }
        }
    }


    public class ProcessingLusherResults
    {
        public PatientResult patientResult = new PatientResult();

        public ProcessingLusherResults(JObject test, TestsResult result, BsonDocument norm)
        {
            string[] testResult = result.Answers[0].ChosenAnswer.Split(' ').Concat(result.Answers[1].ChosenAnswer.Split(' ')).ToArray();

            //заполнение первых 16 шкал — порядок цветов
            AddOrderColor(test, testResult);

            //расчет по формулам
            CalculationByFormulas(test);

            //интерпритация
            RangeInterpretation(norm, patientResult.Scales);

            //Удаляем вспомогательные шкалы из окончательного результата 
            RemoveAuxiliaryScales(patientResult.Scales);
        }

        //первые 16 шкал — порядок цветов
        private void AddOrderColor(JObject test, string[] testResult)
        {
            int start = 0;
            for (int k = 0; k < 2; k++)
            {
                for (int i = start; i < start + 8; i++)
                {
                    var scale = new PatientResult.Scale();

                    if (test["Groups"]["item"][i]["NormID"] != null)
                        scale.IdNormScale = test["Groups"]["item"][i]["NormID"].ToString();
                    scale.IdTestScale = test["Groups"]["item"][i]["ID"].ToString();
                    scale.Name = test["Groups"]["item"][i]["Name"]["#text"].ToString();
                    for (int j = start; j < start + 8; j++)
                    {
                        if (testResult[j] == i.ToString() && i < 8)
                        {
                            scale.Scores = j + 1;
                            break;
                        }
                        if (testResult[j] == (i - 8).ToString() && i >= 8)
                        {
                            scale.Scores = j - 7;
                            break;
                        }
                    }
                    patientResult.Scales.Add(scale);
                }
                start += 8;
            }
        }

        private void CalculationByFormulas(JObject test)
        {
            for (int i = 16; i < 40; i++)
            {
                var patientScale = new PatientResult.Scale();
                var testScale = test["Groups"]["item"][i];

                if (testScale["NormID"] != null)
                    patientScale.IdNormScale = testScale["NormID"].ToString();
                patientScale.IdTestScale = testScale["ID"].ToString();
                patientScale.Name = testScale["Name"]["#text"].ToString();

                string formula = testScale["Formula"].ToString();

                //парсим формулу
                formula = ParseFormula.Parse(formula, patientResult.Scales);

                //расчет баллов по формуле
                patientScale.Scores = Calculator.Calculate(formula);

                patientResult.Scales.Add(patientScale);
            }
        }

        //автоматическая интерпретация результатов
        private void RangeInterpretation(BsonDocument norm, List<PatientResult.Scale> results)
        {
            //шкалы из норм
            BsonArray normScales = new BsonArray();
            if (norm["main"]["groups"]["item"]["quantities"]["item"] is BsonArray)
                foreach (var scale in norm["main"]["groups"]["item"]["quantities"]["item"].AsBsonArray)
                    normScales.Add(scale);
            else
                normScales.Add(norm["main"]["groups"]["item"]["quantities"]["item"]);


            //Для каждой вычисленной шкалы
            foreach (var result in results)
            {
                //если градация еще не определена
                if (result.GradationNumber == null)
                    foreach (var normScale in normScales)
                    {
                        //находим соответствующую шкалу из норм 
                        if (result.Scores != null && result.IdNormScale == normScale["id"].AsString)
                        {
                            //выбираем все градации шкалы
                            BsonArray grads = new BsonArray();
                            if (normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"] is BsonArray)
                                foreach (var grad in normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"].AsBsonArray)
                                    grads.Add(grad);
                            else
                                grads.Add(normScale["treelevel"]["children"]["item"]["treelevel"]["children"]["item"]["termexpr"]["gradations"]["gradations"]["item"]);



                            //Для каждой градации
                            foreach (var bsonGrad in grads)
                            {
                                var grad = JObject.Parse(JsonConvert.SerializeObject(BsonTypeMapper.MapToDotNetValue(bsonGrad)));

                                //если обе границы inf
                                if (grad["lowerformula"]["ftext"].ToString() == "-inf" && grad["upperformula"]["ftext"].ToString() == "+inf")
                                {
                                    if (grad["comment"]["#text"] != null)
                                        result.Interpretation = grad["comment"]["#text"].ToString();
                                    result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                }
                                else
                                //если слева -inf, справа число
                                if (grad["lowerformula"]["ftext"].ToString() == "-inf" && grad["upperformula"]["ftext"].ToString() != "+inf")
                                {
                                    if (result.Scores <= double.Parse(grad["upperformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                                else
                                //если справа +inf, слева число
                                if (grad["lowerformula"]["ftext"].ToString() != "-inf" && grad["upperformula"]["ftext"].ToString() == "+inf")
                                {
                                    if (result.Scores > double.Parse(grad["lowerformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                                else
                                //c обеих сторон числа
                                if (grad["lowerformula"]["ftext"].ToString() != "-inf" && grad["upperformula"]["ftext"].ToString() != "+inf")
                                {
                                    if (result.Scores > double.Parse(grad["lowerformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture) && result.Scores <= (int)double.Parse(grad["upperformula"]["ftext"].ToString(), System.Globalization.CultureInfo.InvariantCulture))
                                    {
                                        if (grad["comment"]["#text"] != null)
                                            result.Interpretation = grad["comment"]["#text"].ToString();
                                        result.GradationNumber = Int32.Parse(grad["number"].ToString());
                                    }
                                }
                            }
                            break;
                        }
                    }
            }
        }

        private void RemoveAuxiliaryScales(List<PatientResult.Scale> scales)
        {
            //удаляем первые 16 значений — место цветов в двух выборках.
            //Вместо них оставим средние значения мест (среднее по ряду выборов место каждого цвета)
            scales.RemoveRange(0, 16);

            //для шкал y1,y2,y3,x1,x2,x3 стрессовых цветов сохраняем только вариант, неравный 0 в соответствии с местом
            for (int i = 15; i < 21; i++)
                if (scales[i].Scores == 0)
                    scales[i].Scores = null;

            //убираем 2 промежуточных значения для рассчета показателя стресса
            scales.RemoveRange(scales.Count() - 3, 2);
        }
    }

    public static class ParseFormula
    {
        public static string Parse(string formula, List<PatientResult.Scale> results)
        {
            int firstIndex;
            do
            {
                firstIndex = formula.IndexOf("GRADNUM");
                if (firstIndex != -1)
                {
                    string idTestScale = "";
                    int lastIndex;
                    for (lastIndex = firstIndex + 11; lastIndex < formula.Length; lastIndex++)
                        if (formula[lastIndex] != ')')
                            idTestScale += formula[lastIndex];
                        else break;

                    int value = 0;
                    foreach (var r in results)
                        if (r.IdTestScale == idTestScale)
                        {
                            value = (int)r.GradationNumber;
                            break;
                        }
                    formula = formula.Remove(firstIndex, lastIndex - firstIndex + 1);
                    formula = formula.Insert(firstIndex, value.ToString());
                }
            } while (firstIndex != -1);

            do
            {
                firstIndex = formula.IndexOf("Scale");
                if (firstIndex != -1)
                {
                    string idTestScale = "";
                    int lastIndex;
                    for (lastIndex = firstIndex + 9; lastIndex < formula.Length; lastIndex++)
                        if (formula[lastIndex] != ')')
                            idTestScale += formula[lastIndex];
                        else break;

                    int value = 0;
                    foreach (var r in results)
                        if (r.IdTestScale == idTestScale)
                        {
                            value = (int)r.Scores;
                            break;
                        }
                    formula = formula.Remove(firstIndex, lastIndex - firstIndex + 1);
                    formula = formula.Insert(firstIndex, value.ToString());
                }
            } while (firstIndex != -1);

            do
            {
                firstIndex = formula.IndexOf("STSUMM");
                if (firstIndex != -1)
                {
                    string idTestScale = "";
                    int lastIndex;
                    for (lastIndex = firstIndex + 10; lastIndex < formula.Length; lastIndex++)
                        if (formula[lastIndex] != ')')
                            idTestScale += formula[lastIndex];
                        else break;

                    int value = 0;
                    foreach (var r in results)
                        if (r.IdTestScale == idTestScale)
                        {
                            value = (int)r.Scores;
                            break;
                        }
                    formula = formula.Remove(firstIndex, lastIndex - firstIndex + 1);
                    formula = formula.Insert(firstIndex, value.ToString());
                }
            } while (firstIndex != -1);

            do
            {
                firstIndex = formula.IndexOf("abs");
                if (firstIndex != -1)
                {
                    int lastIndex;
                    string shortFormula = "";
                    for (lastIndex = firstIndex + 4; lastIndex < formula.Length; lastIndex++)
                        if (formula[lastIndex] != ')')
                            shortFormula += formula[lastIndex];
                        else break;
                    double value = Calculator.Calculate(shortFormula);
                    if (value < 0)
                        value *= -1;

                    formula = formula.Remove(firstIndex, lastIndex - firstIndex + 1);
                    formula = formula.Insert(firstIndex, value.ToString());
                }
            } while (firstIndex != -1);

            do
            {
                firstIndex = formula.IndexOf("==");
                if (firstIndex != -1)
                {
                    int leftPartStart, rightPartFinish;
                    string leftPart = "", rightPart = "";
                    for (leftPartStart = firstIndex - 1; leftPartStart > -1; leftPartStart--)
                        if (formula[leftPartStart] != '(')
                            leftPart += formula[leftPartStart];
                        else break;
                    for (rightPartFinish = firstIndex + 2; rightPartFinish < formula.Length; rightPartFinish++)
                        if (formula[rightPartFinish] != ')')
                            rightPart += formula[rightPartFinish];
                        else break;

                    int value = 0;
                    if (leftPart == rightPart)
                        value = 1;

                    formula = formula.Remove(leftPartStart, rightPartFinish - leftPartStart + 1);
                    formula = formula.Insert(leftPartStart, value.ToString());
                }
            } while (firstIndex != -1);

            do
            {
                firstIndex = formula.IndexOf("\r");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 1);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("\n");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 1);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("\t");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 1);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("out");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 7);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("if ");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 3);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("else");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 4);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("{");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 1);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("}");
                if (firstIndex != -1)
                    formula = formula.Remove(firstIndex, 1);
            } while (firstIndex != -1);
            do
            {
                firstIndex = formula.IndexOf("||");
                if (firstIndex != -1)
                {
                    int r = 0;
                    if (formula[firstIndex - 1] == '1' || formula[firstIndex + 2] == '1')
                        r = 1;
                    formula = formula.Remove(firstIndex - 1, 4);
                    formula = formula.Insert(firstIndex - 1, r.ToString());
                }
            } while (firstIndex != -1);


            return formula;
        }
    }



    //считает арифметическое выражение, записанное в виде строки
    public static class Calculator
    {
        private const string numberChars = "01234567890.";
        private const string operatorChars = "^*/+-";

        public static double Calculate(string expression)
        {
            return EvaluateParenthesis(expression);
        }

        private static double EvaluateParenthesis(string expression)
        {
            string planarExpression = expression;
            while (planarExpression.Contains('('))
            {
                int clauseStart = planarExpression.IndexOf('(') + 1;
                int clauseEnd = IndexOfRightParenthesis(planarExpression, clauseStart);
                string clause = planarExpression.Substring(clauseStart, clauseEnd - clauseStart);
                planarExpression = planarExpression.Replace("(" + clause + ")", EvaluateParenthesis(clause).ToString());
            }
            return Evaluate(planarExpression);
        }

        private static int IndexOfRightParenthesis(string expression, int start)
        {
            int c = 1;
            for (int i = start; i < expression.Length; i++)
            {
                switch (expression[i])
                {
                    case '(': c++; break;
                    case ')': c--; break;
                }
                if (c == 0) return i;
            }
            return -1;
        }

        private static double Evaluate(string expression)
        {
            string normalExpression = expression.Replace(" ", "").Replace(",", ".");
            List<char> operators = normalExpression.Split(numberChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).ToList();
            List<double> numbers = normalExpression.Split(operatorChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToList();

            foreach (char o in operatorChars)
            {
                for (int i = 0; i < operators.Count; i++)
                {
                    if (operators[i] == o)
                    {
                        double result = Calc(numbers[i], numbers[i + 1], o);
                        numbers[i] = result;
                        numbers.RemoveAt(i + 1);
                        operators.RemoveAt(i);
                        i--;
                    }
                }
            }
            return numbers[0];
        }

        private static double Calc(double left, double right, char oper)
        {
            switch (oper)
            {
                case '+': return left + right;
                case '-': return left - right;
                case '*': return left * right;
                case '/': return left / right;
                case '^': return Math.Pow(left, right);
                default: return 0;
            }
        }
    }
}