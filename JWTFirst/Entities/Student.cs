namespace JWTFirst.Entities;

public class Student
{
    public Guid StudentId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
