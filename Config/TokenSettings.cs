
public class JwtSettings
{
    public required TokenSettings Access { get; set; }
    public required TokenSettings Refresh { get; set; }
    public required TokenSettings Invitation { get; set; }

}
public class TokenSettings
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int ExpiryMinutes { get; set; }
}
