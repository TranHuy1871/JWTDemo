using System.ComponentModel.DataAnnotations;

namespace JWTFirst.Entities;

public class RefreshToken
{
    [Key]
    public Guid TokenRefreshId { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }   // id access token
    public bool IsUsed { get; set; }    // check sử dụng
    public bool IsRevoked { get; set; } // check thu hồi
    public DateTime IssueAt { get; set; }   // ngày tạo
    public DateTime ExpiredAt { get; set; } // ngày hh

    //[ForeignKey("StudentId")]
    public Guid StudentId { get; set; }
}
