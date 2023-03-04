using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private Context db;

        public UserRepository(Context db)
        {
            this.db = db;
        }


        //список всех пользователей
        public async Task<IEnumerable<User>> GetAll()
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty; // фильтр для выборки всех документов
            return await db.Users.Find(filter).ToListAsync();
        }

        //пользователь по логину и паролю
        public Task<User> GetIdentity(string username, string password)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty;
            var people = db.Users.Find(filter).ToList();


            var passwordHasher = new PasswordHasher<User>();
            bool verified = false;

            Task<User> user = null;
            foreach (var x in people)
                if (username == x.login)
                {
                    var result = passwordHasher.VerifyHashedPassword(x, x.password, password);
                    if (result == PasswordVerificationResult.Success) verified = true;
                    else if (result == PasswordVerificationResult.SuccessRehashNeeded) verified = true;
                    else if (result == PasswordVerificationResult.Failed) verified = false;
                    if (verified)
                        user = Task.FromResult(x);
                }
            return user;
        }

        //получаем количество страниц с пользователями, если на странице 10 пользователей
        public async Task<double> GetPagesCount()
        {
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty;
            long count = await db.Users.CountDocumentsAsync(filter);
            return Math.Ceiling((double)count / 10.0);
        }

        //получаем часть пользователей для пагинации
        public async Task<IEnumerable<User>> GetWithCount(int pageNumber)
        {
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty;
            List<User> allUsers = await db.Users.Find(filter).ToListAsync();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= allUsers.Count)
                count = allUsers.Count - start;
            User[] page = new User[count];
            allUsers.CopyTo(start, page, 0, count);
            return page;
        }

        //фильтрация пользователей по имени
        public async Task<IEnumerable<User>> GetByName(string value)
        {
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty;
            var allUsers = await db.Users.Find(filter).ToListAsync();
            return allUsers.FindAll(x => x.name.ToLower().Contains(value.ToLower()) == true);
        }

        //получаем количество страниц с пользователями c фильтрацией по имени, если на странице 10 пользователей
        public async Task<double> GetByNamePagesCount(string value)
        {
            var users = await GetByName(value);
            users = users.ToList();
            long count = users.Count();
            return Math.Ceiling((double)count / 10.0);
        }

        //получаем часть пользователей c фильтрацией по имени для пагинации
        public async Task<IEnumerable<User>> GetByNameWithCount(int pageNumber, string name)
        {
            List<User> users = new List<User>();
            var u = await GetByName(name);
            users = u.ToList();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= users.Count)
                count = users.Count - start;
            User[] page = new User[count];
            users.CopyTo(start, page, 0, count);
            return page;
        }

        //получаем пользователя по id
        public async Task<User> GetItemById(string id)
        {
            return await db.Users.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }

        // добавление пользователя
        public async Task Create(User u)
        {
            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(u, u.password);
            u.password = hashedPassword;

            await db.Users.InsertOneAsync(u);
        }

        // обновление пользователя
        public async Task Update(User u)
        {
            BsonDocument doc = new BsonDocument("_id", new ObjectId(u.id));
            User user = await db.Users.Find(new BsonDocument("_id", new ObjectId(u.id))).FirstOrDefaultAsync();
            if (u.password != "")
            {
                var passwordHasher = new PasswordHasher<User>();
                var hashedPassword = passwordHasher.HashPassword(u, u.password);
                u.password = hashedPassword;        
            }
            else u.password = user.password;
            await db.Users.ReplaceOneAsync(doc, u);
        }

        // удаление пользователя
        public async Task Remove(string id)
        {
            await db.Users.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
    }
}