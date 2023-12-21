using JWTFirst.DTOs;
using JWTFirst.Entities;
using JWTFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTFirst.Controllers;

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
    public IActionResult GetAll()
    {
        return Ok(StudentManager.GetStudents());
    }
     

    [HttpPost("RefreshToken")]
    public IActionResult RefreshToken(TokenModel tokenModel)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secrectKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSetting:SecrectKey"]);

        var tokenParam = new TokenValidationParameters
        {
            // tự cấp token
            ValidateIssuer = false,
            ValidateAudience = false,

            // ký vào token
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secrectKeyBytes), // thuật toán đối xứng (tự động mã hóa)

            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = false, // k check token hh
        };

        try
        {
            // check access token valid format
            var tokenInVerification = jwtTokenHandler.ValidateToken(
                tokenModel.AccessToken,
                tokenParam,
                out var validatedToken);

            // check alg
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals
                    (SecurityAlgorithms.HmacSha512,
                    StringComparison.InvariantCultureIgnoreCase);

                if (!result) // false
                {
                    return BadRequest("k đúng alg: " + result);
                }

            }

            // check access token expire
            var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);

            if (expireDate > DateTime.UtcNow)
            {
                return BadRequest("hết hạn");
            }

            // check refreshtoken có trong db k
            var storedToken = StudentManager.GetToken(tokenModel);
            if (storedToken is null)
            {
                return BadRequest("token null");
            }

            // check refreshtoken đã use chưa
            if (storedToken.IsUsed)
            {
                return BadRequest("đã đc sử dụng");
            }

            // check refreshtoken đã revoke chưa
            if (storedToken.IsRevoked)
            {
                return BadRequest("đã bị thu hồi");
            }

            // check access token == jwtid trong refresh token k
            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedToken.JwtId != jti)
            {
                return BadRequest("token đã hh k trùng");
            }

            // update token is used
            storedToken.IsUsed = true;
            storedToken.IsRevoked = true;
             
            StudentManager.UpdateToken(storedToken);

            // create new token
            var student = StudentManager.CreateNewToken(storedToken);
            var token = GenerateToken(student);

            return Ok(token);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);  
        }
    }

    [HttpGet("GetAllToken")]
    public IActionResult GetAllToken()
    {
        return Ok(StudentManager.GetTokens());
    }
     


    private TokenModel GenerateToken(Student student)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var secrectKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSetting:SecrectKey"]);

        var tokenDesc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("StudentId", student.StudentId.ToString()),
                new Claim("UserName", student.UserName),
                new Claim(ClaimTypes.Name, student.Name),
                new Claim("Age", student.Age.ToString()),

                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()), // id access token
                 
                // new Claim("TokenId", Guid.NewGuid().ToString())
            }),

            Expires = DateTime.UtcNow.AddSeconds(9),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secrectKeyBytes),
                SecurityAlgorithms.HmacSha512Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDesc);

        var accessToken = jwtTokenHandler.WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        // save entity refresh token vào db
        var refreshTokenEntity = new RefreshToken
        {
            TokenRefreshId = Guid.NewGuid(),
            JwtId = token.Id,
            StudentId = student.StudentId,
            Token = refreshToken,
            IsUsed = false,
            IsRevoked = false,
            IssueAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(1),
        };
        StudentManager.AddRefreshToken(refreshTokenEntity);


        return new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    // refresh token
    private string GenerateRefreshToken()
    {
        var random = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);

            return Convert.ToBase64String(random);
        }
    }

    // convert để check xem token đã hh chưa
    private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
    {
        var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

        return dateTimeInterval;
    }
}
