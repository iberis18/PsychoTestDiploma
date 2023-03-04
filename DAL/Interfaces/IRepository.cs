using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Interfaces
{
    public interface IRepository<T> where T : class // интерфейс репозитория
    {
        Task<IEnumerable<T>> GetAll(); // получить все элементы
        Task<T> GetItemById(string id); // получить элемент по id
        Task Create(T item); // добавить элемент 
        Task Update(T item); // обновить элемент 
        Task Remove(string id); // удалить элемент 
        Task<double> GetPagesCount(); //получаем количество страниц
        Task<IEnumerable<T>> GetWithCount(int pageNumber); //получаем часть элементов для пагинации
        Task<IEnumerable<T>> GetByName(string value);     //фильтрация пользователей по имени
        Task<double> GetByNamePagesCount(string value); //получаем количество страниц c фильтрацией по имени
        Task<IEnumerable<T>> GetByNameWithCount(int pageNumber, string name); //получаем часть элементов c фильтрацией по имени для пагинации
        Task<T> GetIdentity(string value1, string value2);
    }
}