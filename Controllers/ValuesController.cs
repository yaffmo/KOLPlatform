using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using KOLConsole.Data;
using KOLConsole.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KOLConsole.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public ValuesController(DataContext context, IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
            _context = context;

        }
        // GET api/values
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetValues()
        {
            var value = await _context.Values.ToListAsync();
            return Ok(value);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetValue(int id)
        {
            var value = await _context.Values.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(value);
        }

        // POST api/values
        // [HttpPost]
        // public void Post([FromBody] string value) {}

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO UserForLoginDTO)
        {
            var user = await _repo.Login(UserForLoginDTO.Username, UserForLoginDTO.Password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var claims = new []
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("Appsetting:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTO UserForRegisterDTO)
        {
            UserForRegisterDTO.Username = UserForRegisterDTO.Username.ToLower();

            var userToCreate = new User
            {
                Username = UserForRegisterDTO.Username
            };
            await _repo.Register(userToCreate, UserForRegisterDTO.Password);

            return StatusCode(201);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {}

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) {}
    }
}
