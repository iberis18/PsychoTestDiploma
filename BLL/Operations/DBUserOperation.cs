using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;
using DAL.Interfaces;
using BLL.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BLL.Operations
{
    public class DBUserOperation : IDBUser
    {
        IUnitOfWork db; //репозиторий

        public DBUserOperation(IUnitOfWork db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await db.Users.GetAll();
            return users.Select(i => new User(i)).ToList();
        }

        public async Task<User> GetIdentityUsers(string username, string password)
        {
            return new User(await db.Users.GetIdentity(username, password));
        }

        public async Task<double> GetUsersPagesCount()
        {
            return Math.Ceiling((double)(await db.Users.ItemsCount()) / 10.0);
        }

        public async Task<IEnumerable<User>> GetUsersWithCount(int pageNumber)
        {
            List<User> allUsers = (await db.Users.GetAll()).Select(i => new User(i)).ToList();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= allUsers.Count)
                count = allUsers.Count - start;
            User[] page = new User[count];
            allUsers.CopyTo(start, page, 0, count);
            return page;
        }
        public async Task<IEnumerable<User>> GetUsersByName(string value)
        {
            return (await db.Users.GetAll())
                .ToList()
                .FindAll(x => x.name.ToLower().Contains(value.ToLower()) == true)
                .Select(x => new User(x));
        }
        public async Task<double> GetUsersByNamePagesCount(string value)
        {
            return Math.Ceiling((double)((await GetUsersByName(value)).Count()) / 10.0);
        }
        public async Task<IEnumerable<User>> GetUsersByNameWithCount(int pageNumber, string name)
        {
            List<User> users = (await GetUsersByName(name)).ToList();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= users.Count)
                count = users.Count - start;
            User[] page = new User[count];
            users.CopyTo(start, page, 0, count);
            return page;
        }
        public async Task<User> GetUserById(string id)
        {
            return new User(await db.Users.GetItemById(id));
        }
        public async Task CreateUser(User u)
        {
            await db.Users.Create(new DAL.Models.User()
                { id = u.Id, password = u.Password, name = u.Name, login = u.Login, role = u.Role });
        }
        public async Task UpdateUser(User u)
        {
            await db.Users.Update(new DAL.Models.User() { id = u.Id, password = u.Password, name = u.Name, login = u.Login, role = u.Role });
        }
        public async Task RemoveUser(string id)
        {
            await db.Users.Remove(id);
        }
    }
}