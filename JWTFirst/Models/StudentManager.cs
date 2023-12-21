using JWTFirst.DTOs;
using JWTFirst.Entities;

namespace JWTFirst.Models;

public class StudentManager
{
    private static List<Student> Students = new List<Student>();

    private static List<RefreshToken> RefreshTokens = new List<RefreshToken>();

    // student
    public static void AddStudent(StudentDTO studentDTO)
    {
        Student student = new Student
        {
            StudentId = Guid.NewGuid(),
            UserName= studentDTO.UserName,
            Password= studentDTO.Password,  
            Name = studentDTO.Name,
            Age= studentDTO.Age,
        };
        Students.Add(student);
    }

    public static bool AuthenStudent(StudentLoginDTO studentLoginDTO)
    {
        try
        {
            return Students.Exists(student =>
                student.UserName == studentLoginDTO.UserName && student.Password == studentLoginDTO.Password);
             
        }
        catch (Exception)
        {
            throw;
        }

    }

    public static Student LoginStudent(StudentLoginDTO studentLoginDTO)
    {
        if (AuthenStudent(studentLoginDTO))
        {
            return Students.Find(student =>
                  student.UserName == studentLoginDTO.UserName && student.Password == studentLoginDTO.Password);

        }
        return null;
    }

    public static List<Student> GetStudents()
    {
        return Students.ToList();
    } 



    // lưu refresh token vào csdl
    public static void AddRefreshToken(RefreshToken refreshTokenDTO)
    {
        RefreshToken refreshToken = new RefreshToken
        {
            TokenRefreshId = refreshTokenDTO.TokenRefreshId,
            Token = refreshTokenDTO.Token,
            JwtId = refreshTokenDTO.JwtId,
            IsUsed= refreshTokenDTO.IsUsed,
            IsRevoked= refreshTokenDTO.IsRevoked,
            IssueAt= refreshTokenDTO.IssueAt,
            ExpiredAt= refreshTokenDTO.ExpiredAt,
        };
        RefreshTokens.Add(refreshTokenDTO);
    }

    // làm mới token 
    public static RefreshToken GetToken(TokenModel tokenModel)
    {
        var refreshToken = RefreshTokens.FirstOrDefault(x => x.Token == tokenModel.RefreshToken);

        return refreshToken;
    }

    // update lại token khi đã được sử dụng r
    public static void UpdateToken(RefreshToken refreshTokenDTO)
    {
        var existingToken = RefreshTokens.FirstOrDefault(t => t.TokenRefreshId == refreshTokenDTO.TokenRefreshId);

        if (existingToken != null)
        {
            // update in4 của RefreshToken đã tồn tại
            existingToken.Token = refreshTokenDTO.Token;
            existingToken.JwtId = refreshTokenDTO.JwtId;
            existingToken.IsUsed = refreshTokenDTO.IsUsed;
            existingToken.IsRevoked = refreshTokenDTO.IsRevoked;
            existingToken.IssueAt = refreshTokenDTO.IssueAt;
            existingToken.ExpiredAt = refreshTokenDTO.ExpiredAt;
        }
    }

    // thêm mói token
    public static Student CreateNewToken(RefreshToken refreshTokenDTO)
    {
        return Students.SingleOrDefault(st => st.StudentId == refreshTokenDTO.StudentId);
    }

    // get all
    public static List<RefreshToken> GetTokens()
    {
        return RefreshTokens.ToList();
    }
}
