using System.ComponentModel.DataAnnotations;

public class AccessPayloadDTO
{
    [Required]
    public required int Id { get; set; }
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



public class RefreshPayloadDTO
{
    [Required]
    public required int Id { get; set; }
}

public class InvitationPayloadDTO
{
    [Required]
    public required int Id { get; set; }
}