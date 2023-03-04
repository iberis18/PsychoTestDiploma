using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Interfaces
{
    public interface ITestRepository<T> where T : class // интерфейс репозитория
    {
        Task<IEnumerable<T>> GetAll(); // получить все элементы
        Task<string> GetItemById(string id); // получить элемент по id
        Task<string> GetItemByIdWithoutImages(string id); //получаем тест по id
        Task<IEnumerable<T>> GetTestsByPatientToken(Patient patient); //получаем все назначенные пациенту тесты 
        Task<string> ImportTestFile(string file); //Импорт теста
        Task ImportNormFile(string file, string testId);//Импорт норм
        Task<IEnumerable<string>> GetNorms(); //получаем список id всех норм
        Task Remove(string id);  // удаление теста вместе с нормой и изображениями
        Task ImportImage(Stream imageStream, string imageName); // сохранение изображения
    }
}