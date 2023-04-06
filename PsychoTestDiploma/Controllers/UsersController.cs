using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.Operations;
using DAL.Models;
using BLL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using User = BLL.Models.User;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PsychoTestDiploma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUser db;
        public UsersController(Context context)
        {
            db = new UserOperations(new DBUnitOfWork(context));
        }

        //получение всех пользователей
        // GET: api/<UsersController>
        //[Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            return await db.GetUsers();
        }

        //получение пользователя по id
        // GET api/<UsersController>/62a1f08829de97df5563051f
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<User> Get(string id)
        {
            return await db.GetUserById(id);
        }

        //получение общего количества страниц с пользователями
        // GET: api/<UsersController>/pageCount
        [Authorize(Roles = "admin")]
        [Route("pageCount")]
        [HttpGet]
        public async Task<double> GetPagesCount()
        {
            return await db.GetUsersPagesCount();
        }

        //получение списка пользователей на конкретной странице
        // GET api/<UsersController>/page/3
        [Authorize(Roles = "admin")]
        [HttpGet("page/{value}")]
        public async Task<IEnumerable<User>> GetWithCount(int value)
        {
            return await db.GetUsersWithCount(value);
        }

        //получение пользователей с подстрокой value в имени
        // GET api/<UsersController>/name/value
        [Authorize(Roles = "admin")]
        [HttpGet("name/{value}")]
        public async Task<IEnumerable<User>> GetByName(string value)
        {
            return await db.GetUsersByName(value);
        }

        //получение общего количества страниц с пользователями c фильтрацией по имени
        // GET: api/<UsersController>/name/pageCount/value
        [Authorize(Roles = "admin")]
        [HttpGet("name/pageCount/{value}")]
        public async Task<double> GetByNamePagesCount(string value)
        {
            return await db.GetUsersByNamePagesCount(value);
        }

        //получение списка пользователей на конкретной странице с фильтрацией по имени
        // GET api/<UsersController>/name/page/3/value
        [Authorize(Roles = "admin")]
        [HttpGet("name/page/{pageValue}/{nameValue}")]
        public async Task<IEnumerable<User>> GetByNameWithCount(int pageValue, string nameValue)
        {
            return await db.GetUsersByNameWithCount(pageValue, nameValue);
        }

        // POST api/<UsersController>
        //[Authorize]
        [HttpPost]
        public async Task Post([FromBody] User value)
        {
            await db.CreateUser(value);
        }

        // PUT api/<UsersController>/62a1f08829de97df5563051f
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] User value)
        {
            await db.UpdateUser(value);
        }

        // DELETE api/<UsersController>/62a1f08829de97df5563051f
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            await db.RemoveUser(id);
        }
    }
}
