using JWTFirst.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace JWTFirst.Models;

public class StudentManager
{
    private static List<Student> Students = new List<Student>();

    public static void AddStudent(StudentDTO studentDTO)
    {
        Student student = new Student
        {
            Id = Guid.NewGuid(),
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
}
