using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BLL.Authorisation;
using BLL.Operations;
using DAL.Models;
using BLL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace PsychoTestDiploma.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBPatientOperation db;
        public AccountController(Context context)
        {
            db = new DBPatientOperation(new DBUnitOfWork(context));
        }
        [HttpPost("/authentication")]
        public IActionResult Token([FromBody] BLL.Models.User user)
        {
            var identity = GetIdentity(user.Login, user.Password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                accessToken = encodedJwt,
                username = identity.Name
            };

            return Json(response);
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            //BLL.Models.User person = db.GetIdentityUsers(username, password);
            //if (person != null)
            //{
            //    var claims = new List<Claim>
            //    {
            //        new Claim(ClaimsIdentity.DefaultNameClaimType, person.login),
            //        new Claim(ClaimsIdentity.DefaultRoleClaimType, person.role)
            //    };
            //    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            //    return claimsIdentity;
            //}

            // если пользователя не найдено
            return null;
        }
    }
}