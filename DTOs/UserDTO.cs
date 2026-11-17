using System.ComponentModel.DataAnnotations;

public class UserDTO
{
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    [EnumDataType(typeof(RoleE))]
    public required RoleE Role { get; set; }
}