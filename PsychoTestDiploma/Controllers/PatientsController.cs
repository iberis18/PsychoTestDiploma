using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.Operations;
using DAL.Models;
using BLL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Patient = BLL.Models.Patient;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PsychoTestDiploma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatient db;
        public PatientsController(Context context)
        {
            db = new PatientOperations(new DBUnitOfWork(context));
        }

        //получение всех пациентов
        // GET: api/<PatientsController>
        //[Authorize]
        [HttpGet]
        public async Task<IEnumerable<Patient>> Get()
        {
            return await db.GetPatients();
        }

        //получение пациента по id
        // GET api/<PatientsController>/62a1f08829de97df5563051f
        //[Authorize]
        [HttpGet("{id}")]
        public async Task<Patient> Get(string id)
        {
            return await db.GetPatientById(id);
        }

        //получение общего количества страниц с пациентами
        // GET: api/<PatientsController>/pageCount
        //[Authorize]
        [Route("pageCount")]
        [HttpGet]
        public async Task<double> GetPagesCount()
        {
            return await db.GetPatientsPagesCount();
        }

        //получение списка пациентов на конкретной странице
        // GET api/<PatientsController>/page/3
        //[Authorize]
        [HttpGet("page/{value}")]
        public async Task<IEnumerable<Patient>> GetWithCount(int value)
        {
            return await db.GetPatientsWithCount(value);
        }

        //получение пациентов с подстрокой value в имени
        // GET api/<PatientsController>/name/value
        //[Authorize]
        [HttpGet("name/{value}")]
        public async Task<IEnumerable<Patient>> GetByName(string value)
        {
            return await db.GetPatientsByName(value);
        }

        //получение общего количества страниц с пациентами c фильтрацией по имени
        // GET: api/<PatientsController>/name/pageCount/value
        //[Authorize]
        [HttpGet("name/pageCount/{value}")]
        public async Task<double> GetByNamePagesCount(string value)
        {
            return await db.GetPatientsByNamePagesCount(value);
        }

        //получение списка пациентов на конкретной странице с фильтрацией по имени
        // GET api/<PatientsController>/name/page/3/value
        //[Authorize]
        [HttpGet("name/page/{pageValue}/{nameValue}")]
        public async Task<IEnumerable<Patient>> GetByNameWithCount(int pageValue, string nameValue)
        {
            return await db.GetPatientsByNameWithCount(pageValue, nameValue);
        }

        //получение пациента с отфильтрованными результатами для статистики
        // GET: api/<PatientsController>/results/62a1f08829de97df5563051f/62a1f08829de97df5563051f
        //[Authorize]
        [HttpGet("results/{patientId}/{testId}")]
        public async Task<Patient> GetResultsByTestId(string patientId, string testId)
        {
            return await db.GetPatientsResultsByTestId(patientId, testId);
        }

        // POST api/<PatientsController>
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Patient value)
        {
            string token = await db.CreatePatient(value);
            if (token != null)
            {
                var msg = new { message = "ptest://https://" + this.HttpContext.Request.Host + "/api/link/t=" + token };
                return Ok(msg);
            }
            else return null;
        }

        // PUT api/<PatientsController>/62a1f08829de97df5563051f
        //[Authorize]
        [HttpPut("{id}")]
        public async Task Put([FromBody] Patient value)
        {
            await db.UpdatePatient(value);
        }

        // DELETE api/<PatientsController>/62a1f08829de97df5563051f
        //[Authorize]
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            await db.RemovePatient(id);
        }
    }
}
