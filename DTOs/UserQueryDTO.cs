using System.ComponentModel.DataAnnotations;

public class UserQueryDTO
{
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    [EmailAddress]
    public string? Email { get; set; } = null;
    [EnumDataType(typeof(RoleE))]
    public RoleE? Role { get; set; } = null;
    public bool? IsActive { get; set; } = null;
    public DateTime? CreatedBefore { get; set; } = null;
    public DateTime? CreatedAfter { get; set; } = null;
}