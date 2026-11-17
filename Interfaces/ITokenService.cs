public interface ITokenService
{
    public CookieOptions AccessCookieOptions { get; }
    public CookieOptions RefreshCookieOptions { get; }
    public string GenerateAccessToken(AccessPayloadDTO payload);
    public string GenerateRefreshToken(RefreshPayloadDTO payload);
    public string GenerateInvitationToken(InvitationPayloadDTO payload);
    public RefreshPayloadDTO? VerifyRefreshToken(string tokenString);
    public InvitationPayloadDTO? VerifyInvitationToken(string tokenString);
}