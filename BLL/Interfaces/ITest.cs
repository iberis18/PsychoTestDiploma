using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface ITest
    {
        //получаем краткий список всех тестов в формате id-название-заголовок-инструкция
        Task<IEnumerable<Test>> GetTestsView();

        //получаем тест по id
        Task<string> GetTestById(string id);

        //Импорт теста
        Task<string> ImportTestFile(string file);

        //Импорт норм
        Task ImportNormFile(string file, string testId);

        // удаление теста вместе с нормой и изображениями
        Task RemoveTest(string id);

        // сохранение изображения
        Task ImportImage(Stream imageStream, string imageName);
    }
}