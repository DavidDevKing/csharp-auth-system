using System.ComponentModel.DataAnnotations;

public class AccountActivationDTO
{
    [Required]
    public required string Token { get; set; }
    [Required]
    public required string Password { get; set; }
}