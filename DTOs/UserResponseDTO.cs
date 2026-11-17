
using System.ComponentModel.DataAnnotations;

public class UserResponseDTO(User u)
{
    public int Id { get; set; } = u.Id;
    public string FirstName { get; set; } = u.FirstName;
    public string LastName { get; set; } = u.LastName;
    public string Email { get; set; } = u.Email;
    [EnumDataType(typeof(RoleE))]
    public RoleE Role { get; set; } = u.Role;
    public bool IsActive { get; set; } = u.IsActive;
    public DateTime CreatedAt { get; set; } = u.CreatedAt;
}