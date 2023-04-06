using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.Models;
using BLL.Operations;
using DAL.Models;
using DAL.Repositories;
using Patient = BLL.Models.Patient;

namespace PsychoTestDiploma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly IPatient db;
        private readonly ResultsOperations results;
        public AnswersController(Context context)
        {
            db = new PatientOperations(new DBUnitOfWork(context));
            results = new ResultsOperations(new DBUnitOfWork(context));
        }

        // POST api/<AnswersController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TestsResult value)
        {
            if (this.HttpContext.Request.Headers["Authorization"].ToString() == null)
                return Unauthorized();
            else
            {
                string token = this.HttpContext.Request.Headers["Authorization"].ToString();
                Patient patient = await db.GetPatientByToken(token);
                if (patient == null)
                    return Forbid();
                else
                {
                    //сразу удаляем тест из доступных
                    patient.Tests.Remove(value.Id);
                    await db.UpdatePatient(patient);
                    //Расчет баллов
                    await results.Processing(value, patient);
                }
            }
            return Ok();
        }
    }
}
