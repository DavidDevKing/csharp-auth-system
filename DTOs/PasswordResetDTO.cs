using System.ComponentModel.DataAnnotations;

public class PasswordResetDTO
{
    [Required]
    public required int Id { get; set; }
    [Required]
    public required string Password { get; set; }
}