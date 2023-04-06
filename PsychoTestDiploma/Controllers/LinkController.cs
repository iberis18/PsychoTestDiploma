using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using BLL.Operations;
using DAL.Models;
using DAL.Repositories;
using Patient = BLL.Models.Patient;


namespace PsychoTestDiploma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        private readonly PatientOperations db;
        private readonly TokenOperations tokenOperations;
        public LinkController(Context context)
        {
            db = new PatientOperations(new DBUnitOfWork(context));
            tokenOperations = new TokenOperations();
        }

        //генерация уникальной ссылки на привязку
        // GET api/<LinkController>/generateUrl/62a1f08829de97df5563051f
        [Authorize]
        [HttpGet("generateUrl/{id}")]
        public async Task<string> GenerateUrl(string id)
        {
            Patient p = await db.GetPatientById(id);
            p.Token = tokenOperations.GenerateToken();
            await db.UpdatePatient(p);
            return "ptest://https://" + this.HttpContext.Request.Host + "/api/link/t=" + p.Token;
        }

        //получение уникальной ссылки на привязку
        // GET api/<LinkController>/getUrl/62a1f08829de97df5563051f
        [Authorize]
        [HttpGet("getUrl/{id}")]
        public async Task<string> GetUrl(string id)
        {
            Patient p = await db.GetPatientById(id);
            if (p.Token == null)
            {
                p.Token = tokenOperations.GenerateToken();
                await db.UpdatePatient(p);
            }
            return "ptest://https://" + this.HttpContext.Request.Host + "/api/link/t=" + p.Token;
        }

        //привязка по ссылке
        // GET api/<LinkController>/t={token}
        [HttpGet("{token}")]
        public async Task<IActionResult> Authentication(string token)
        {
            Patient p = await db.GetPatientByToken(token.Remove(0, 2));
            if (p != null)
            {
                //перезаписываем токен, тем самым обеспечивая сгорание ссылки
                p.Token = tokenOperations.GenerateToken();
                await db.UpdatePatient(p);
                var domainName = this.HttpContext.Request.Host;
                var msg = new { token = p.Token, domainName = "https://" + domainName + "/" };
                return Ok(msg);
            }
            else return NoContent();
        }
    }
}
