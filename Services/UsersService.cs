using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
public class UsersService(
    AppDbContext context,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<JwtSettings> jwtSettingsOptions,
    ILogger<UsersService> logger,
    IEmailService emailService) : IUsersService
{
    private readonly JwtSettings jwtSettings = jwtSettingsOptions.Value;
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext;


    public async Task<ServiceResult<UserResponseDTO>> GetUserById(int id)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return ServiceResult.NotFound($"User with id {id} not found.");
        }
        return ServiceResult.Ok(new UserResponseDTO(user));
    }

    public async Task<ServiceResult<UserResponseDTO>> CreateUser(UserDTO userData)
    {
        User user;
        InvitationPayloadDTO invitationPayload;
        string invitationTokenString;
        // Create the user
        try
        {
            user = new()
            {
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                Email = userData.Email,
                Role = userData.Role
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Update the user's invitation string with they're ID
            invitationPayload = new()
            {
                Id = user.Id
            };
            invitationTokenString = tokenService.GenerateInvitationToken(invitationPayload);
            user.InvitationToken = TokenHasher.Hash(invitationTokenString);
            user.TokenExpires = DateTime.UtcNow.AddMinutes(jwtSettings.Invitation.ExpiryMinutes);
            await context.SaveChangesAsync();

        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return ServiceResult.Conflict($"User with email {userData.Email} already exists.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update database");
            return ServiceResult.InternalServerError();
        }


        // Send the Inviation email
        if (!await emailService.SendInvitationMail($"{user.FirstName} {user.LastName}", user.Email, invitationTokenString))
        {
            return ServiceResult.Ok(new UserResponseDTO(user), ["Invitation Email not sent"]);
        }

        return ServiceResult.Ok(new UserResponseDTO(user));
    }

    public async Task<ServiceResult> ResendInvitationMail(int id)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return ServiceResult.NotFound($"User with ID {id} not found");
            if (user.IsActive) return ServiceResult.Conflict("User is already activated");
            string newToken = tokenService.GenerateInvitationToken(
                new()
                {
                    Id = user.Id
                });
            user.InvitationToken = TokenHasher.Hash(newToken);
            user.TokenExpires = DateTime.UtcNow.AddMinutes(jwtSettings.Invitation.ExpiryMinutes);
            await context.SaveChangesAsync();

            if (!await emailService.SendInvitationMail($"{user.FirstName} {user.LastName}", user.Email, newToken)) return ServiceResult.InternalServerError();

            return ServiceResult.Success(StatusCodes.Status204NoContent);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send invitaion email");
            return ServiceResult.InternalServerError();
        }
    }


    public async Task<ServiceResult> DeleteUser(int id)
    {
        if (id == httpContext.User.ToAccessPayload()?.Id) return ServiceResult.Conflict("Cannot Delete Self");
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return ServiceResult.NotFound($"User with ID {id} not found.");
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return ServiceResult.NoContent();
    }



    public async Task<ServiceResult<List<UserResponseDTO>>> FindUsers(UserQueryDTO query)
    {

        try
        {
            var queryable = context.Users.AsQueryable();

            string? firstName = query.FirstName?.ToLowerInvariant();
            string? lastName = query.LastName?.ToLowerInvariant();
            string? email = query.Email?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(query.FirstName)) queryable = queryable.Where(u => u.FirstName.ToLower() == firstName); // TODO: Implement some kind of fuzzy search algorithim for the names and emails
            if (!string.IsNullOrEmpty(query.LastName)) queryable = queryable.Where(u => u.LastName.ToLower() == lastName);
            if (!string.IsNullOrEmpty(query.Email)) queryable = queryable.Where(u => u.Email.ToLower() == email);
            if (query.Role.HasValue) queryable = queryable.Where(u => u.Role == query.Role);
            if (query.IsActive.HasValue) queryable = queryable.Where(u => u.IsActive == query.IsActive);
            if (query.CreatedAfter.HasValue) queryable = queryable.Where(u => u.CreatedAt >= query.CreatedAfter);
            if (query.CreatedBefore.HasValue) queryable = queryable.Where(u => u.CreatedAt <= query.CreatedBefore);


            List<UserResponseDTO> users = await queryable
                .Select(u => new UserResponseDTO(u))
                .ToListAsync();
            return ServiceResult.Ok(users);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch users");
            return ServiceResult.InternalServerError();
        }
    }
}