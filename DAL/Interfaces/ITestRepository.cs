using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DAL.Models;
using MongoDB.Bson;

namespace DAL.Interfaces
{
    public interface ITestRepository<T> where T : class // интерфейс репозитория
    {
        Task<IEnumerable<T>> GetAll(); // получить все элементы
        Task<string> GetItemById(string id); // получить элемент по id
        Task<BsonDocument> GetBsonItemByIRId(string id);
        Task<BsonDocument> GetBsonItemById(string id);
        Task<string> GetItemByIdWithoutImages(string id); //получаем тест по id
        //Task<IEnumerable<T>> GetTestsByPatientToken(Patient patient); //получаем все назначенные пациенту тесты 
        Task ImportTestFile(string file); //Импорт теста
        Task ImportNormFile(string file);//Импорт норм
        Task<IEnumerable<string>> GetNorms(); //получаем список id всех норм
        Task<BsonDocument> GetNormByIRId(string id);
        Task Remove(string id);  // удаление теста вместе с нормой и изображениями
        Task ImportImage(Stream imageStream, string imageName); // сохранение изображения
        //Task Create(string file); // добавить элемент 
        
    }
}