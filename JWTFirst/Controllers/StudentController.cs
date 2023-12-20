using JWTFirst.DTOs;
using JWTFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(StudentDTO studentDTO)
        { 
            StudentManager.AddStudent(studentDTO);

            return Ok("Student registered successfully");
        }

        [HttpPost("Login")]
        public IActionResult Login(StudentLoginDTO studentLoginDTO)
        {
            Student student = StudentManager.LoginStudent(studentLoginDTO);

            if (student == null)
            {
                return BadRequest();
            }

            // cấp token
            var token = GenerateToken(student);

            return Ok(token);
        }

        [HttpGet("GetAll")]
        [Authorize] 
        public IActionResult GetAll() {
             
            return Ok(StudentManager.GetStudents());
        }



        private string GenerateToken(Student student)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secrectKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSetting:SecrectKey"]);

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", student.Id.ToString()),
                    new Claim("UserName", student.UserName),
                    new Claim(ClaimTypes.Name, student.Name),
                    new Claim("Age", student.Age.ToString()),

                    // roles
                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),

                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secrectKeyBytes), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDesc);

            return jwtTokenHandler.WriteToken(token);
        }
    }
}
