using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class AuthService(
    AppDbContext context,
    ILogger<AuthService> logger,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<JwtSettings> jwtSettingsOptions
) : IAuthService
{
    private readonly JwtSettings jwtSettings = jwtSettingsOptions.Value;
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext;


    public async Task<ServiceResult> Login(LoginDTO loginDetails)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginDetails.Email);
        const string dummyHash = "$2a$11$K9bK9bK9bK9bK9bK9bK9b.O6vXz26b0N12GZ3kQ9U58qY2Xp2Z3G.";
        bool passwordsMatch = BCrypt.Net.BCrypt.EnhancedVerify(loginDetails.Password, user != null ? user.Password : dummyHash);
        if (user == null || !passwordsMatch)
        {
            return ServiceResult.Unauthorized("Invalid Username or Password");
        }

        RefreshPayloadDTO refreshPayload = new() { Id = user.Id };
        AccessPayloadDTO accessPayload = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
        // Save tokens to cookies
        string accessTokenString = tokenService.GenerateAccessToken(accessPayload);
        string refreshTokenString = tokenService.GenerateRefreshToken(refreshPayload);
        httpContext.Response.Cookies.Append("access_token", accessTokenString, tokenService.AccessCookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshTokenString, tokenService.RefreshCookieOptions);

        // Save the refresh token in the database
        RefreshToken refreshToken = new()
        {
            User = user,
            UserId = user.Id,
            TokenString = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Refresh.ExpiryMinutes)
        };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();
        return ServiceResult.NoContent();
    }


    public async Task<ServiceResult> Logout()
    {
        AccessPayloadDTO? payload = httpContext.User.ToAccessPayload();
        if (payload == null) return ServiceResult.Unauthorized();

        // Delete the refresh tokens
        await context.RefreshTokens
            .Where(t => t.UserId == payload.Id)
            .ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Remove tokens from cookies
        httpContext.Response.Cookies.Delete("access_token", tokenService.AccessCookieOptions);
        httpContext.Response.Cookies.Delete("refresh_token", tokenService.RefreshCookieOptions);

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> Refresh()
    {
        if (!httpContext.Request.Cookies.TryGetValue("refresh_token", out string? oldRefreshTokenString)) return ServiceResult.Unauthorized();
        RefreshPayloadDTO? payload = tokenService.VerifyRefreshToken(oldRefreshTokenString);
        if (payload == null) return ServiceResult.Unauthorized();

        User? user = context.Users.FirstOrDefault(u => u.Id == payload.Id);
        if (user == null) return ServiceResult.Unauthorized();

        // Generate the tokens
        RefreshPayloadDTO refreshPayload = new()
        {
            Id = user.Id
        };
        AccessPayloadDTO accessPayload = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
        string accessTokenString = tokenService.GenerateAccessToken(accessPayload);
        string refreshTokenString = tokenService.GenerateRefreshToken(refreshPayload);


        context.RefreshTokens.Add(new()
        {
            User = user,
            UserId = user.Id,
            TokenString = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Refresh.ExpiryMinutes)
        });

        await context.SaveChangesAsync();

        httpContext.Response.Cookies.Append("access_token", accessTokenString, tokenService.AccessCookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshTokenString, tokenService.RefreshCookieOptions);



        return ServiceResult.NoContent();
    }



    public async Task<ServiceResult<AccessPayloadDTO>> VerifyInvitationToken(TokenStringDTO tokenDTO)
    {
        string token = tokenDTO.Token;
        string tokenHash = TokenHasher.Hash(token);
        // logger.LogInformation($"{tokenHash}");
        User? user = await context.Users.FirstOrDefaultAsync(u => u.InvitationToken == tokenHash);
        if (user == null) return ServiceResult.Unauthorized();
        if (user.IsActive) return ServiceResult.Conflict("User Already Activated");
        if (user.TokenExpires <= DateTime.UtcNow) return ServiceResult.Unauthorized("Token Expired");
        AccessPayloadDTO accessPayload = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
        return ServiceResult.Ok(accessPayload);
    }


    public async Task<ServiceResult> ActivateAccount(AccountActivationDTO activationCreds)
    {
        string tokenHash = TokenHasher.Hash(activationCreds.Token);
        User? user = await context.Users.FirstOrDefaultAsync(u => u.InvitationToken == tokenHash);
        if (user == null) return ServiceResult.Unauthorized();
        if (user.IsActive) return ServiceResult.Conflict("User Already Activated");
        if (user.TokenExpires <= DateTime.UtcNow) return ServiceResult.Unauthorized("Token Expired");

        user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(activationCreds.Password);
        user.IsActive = true;
        user.InvitationToken = null;
        user.TokenExpires = null;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update database");
            return ServiceResult.InternalServerError();
        }
        return ServiceResult.NoContent();
    }

}