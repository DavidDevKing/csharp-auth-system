using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum RoleE
{
    USER,
    MANAGER,
    ADMIN,
}

[Index(nameof(Id), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(InvitationToken), IsUnique = true)]
public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [EmailAddress]
    [Required]
    public required string Email { get; set; }
    public string? Password { get; set; }
    [EnumDataType(typeof(RoleE))]
    public RoleE Role { get; set; } = RoleE.USER;
    public bool IsActive { get; set; } = false;
    public string? InvitationToken { get; set; }
    public DateTime? TokenExpires { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}

public class RefreshToken
{
    public int Id { get; set; }
    [Required]
    public required string TokenString { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public required DateTime ExpiresAt { get; set; }
    [Required]
    public required int UserId { get; set; }
    [Required]
    public required User User { get; set; }
}