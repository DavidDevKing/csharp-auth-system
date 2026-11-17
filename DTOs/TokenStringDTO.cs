using System.ComponentModel.DataAnnotations;

public class TokenStringDTO
{
    [Required]
    public required string Token { get; set; }
}