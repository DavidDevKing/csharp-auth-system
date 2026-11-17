
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class TokenService(IOptions<JwtSettings> jwtSettingsOptions, IWebHostEnvironment env, ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtSettings jwtSettings = jwtSettingsOptions.Value;

    public CookieOptions AccessCookieOptions
    {
        get
        {
            return new()
            {
                HttpOnly = true,
                Secure = env.IsProduction(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.Access.ExpiryMinutes)
            };
        }
    }
    public CookieOptions RefreshCookieOptions
    {
        get
        {
            return new()
            {
                HttpOnly = true,
                Secure = env.IsProduction(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.Refresh.ExpiryMinutes),
                Path = "/api/auth/refresh"
            };
        }
    }


    private string GenerateToken(List<Claim> claims, TokenSettings tokenSettings)
    {
        var now = DateTime.UtcNow;
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: tokenSettings.Issuer,
            audience: tokenSettings.Audience,
            claims: claims,
            signingCredentials: creds,
            notBefore: now,
            expires: now.AddMinutes(tokenSettings.ExpiryMinutes)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public string GenerateAccessToken(AccessPayloadDTO payload)
    {
        List<Claim> claims = new([
            new (JwtRegisteredClaimNames.Sub, payload.Id.ToString()),
            new (JwtRegisteredClaimNames.GivenName, payload.FirstName),
            new (JwtRegisteredClaimNames.FamilyName, payload.LastName),
            new (JwtRegisteredClaimNames.Email, payload.Email),
            new (ClaimTypes.Role, payload.Role.ToString())
        ]);
        return GenerateToken(claims, jwtSettings.Access);
    }
    public string GenerateRefreshToken(RefreshPayloadDTO payload)
    {
        List<Claim> claims = new([
            new (JwtRegisteredClaimNames.Sub, payload.Id.ToString())
        ]);
        return GenerateToken(claims, jwtSettings.Refresh);
    }

    public string GenerateInvitationToken(InvitationPayloadDTO payload)
    {
        List<Claim> claims = new([
            new (JwtRegisteredClaimNames.Sub, payload.Id.ToString())
        ]);
        return GenerateToken(claims, jwtSettings.Invitation);
    }




    private ClaimsPrincipal? VerifyToken(string tokenString, TokenSettings tokenSettings)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenSettings.Issuer,
            ValidAudience = tokenSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)

        };
        JwtSecurityTokenHandler handler = new() { MapInboundClaims = false };
        try
        {
            ClaimsPrincipal principal = handler.ValidateToken(tokenString, parameters, out SecurityToken token);
            return principal;
        }
        catch (Exception e)
        {
            logger.LogError($"Access Token Verification failed: {e.Message}");
            return null;
        }

    }
    public RefreshPayloadDTO? VerifyRefreshToken(string tokenString)
    {
        ClaimsPrincipal? principal = VerifyToken(tokenString, jwtSettings.Refresh);
        if (principal == null) return null;
        if (!int.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int id))
        {
            logger.LogError("Failed to parse values.");
            return null;
        }
        return new RefreshPayloadDTO()
        {
            Id = id,
        };
    }
    public InvitationPayloadDTO? VerifyInvitationToken(string tokenString)
    {
        ClaimsPrincipal? principal = VerifyToken(tokenString, jwtSettings.Invitation);
        if (principal == null) return null;
        if (!int.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int id))
        {
            logger.LogError("Failed to parse values.");
            return null;
        }
        return new InvitationPayloadDTO()
        {
            Id = id,
        };
    }

}




// HACK: Parses AccessToken Payloads from claims principals. For use with HttpContext.User. Still deciding if this implementation is Sufficient.
public static class ClaimsPrincipalExtensions
{
    public static AccessPayloadDTO? ToAccessPayload(this ClaimsPrincipal user)
    {
        string? roleString = user.FindFirst(ClaimTypes.Role)?.Value;
        string? firstName = user.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
        string? lastName = user.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;
        string? email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (!int.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int id) || !Enum.TryParse<RoleE>(roleString, out RoleE role))
        {
            Console.WriteLine("Failed to parse values.");
            return null;
        }
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Null or empty values in credentials");
            return null;
        }
        return new AccessPayloadDTO()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role
        };
    }
}
