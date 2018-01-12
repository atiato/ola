using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAPIAuth.Models;

namespace WebAPIAuth.Controllers
{
    [Produces("application/json")]
    [Route("api/Token")]
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;
        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Post([FromBody]LoginModel loginViewModel)
        {
            //int userid = GetUserIdFromCredentials(loginViewModel);
            //if(userid == -1)
            //{
            //    return Unauthorized();
            //}

            JwtSecurityToken token = new JwtSecurityToken
               (
                   issuer: "MiraRND",//_configuration["Issuer"],
                   audience: "MiraRND", //_configuration["Audience"],
                   claims: null,
                   expires: DateTime.UtcNow.AddDays(60),
                   notBefore: DateTime.UtcNow,
                   signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UniqueKey")),
                        SecurityAlgorithms.HmacSha256)
               );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        private int GetUserIdFromCredentials(LoginModel loginViewModel)
        {
            var userId = -1;
            if (loginViewModel.Username == "mira" && loginViewModel.Password == "P@ssw0rd")
            {
                userId = 5;
            }

            return userId;
        }
    }
}