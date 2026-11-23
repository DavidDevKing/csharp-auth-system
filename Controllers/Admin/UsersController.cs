using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace csharp_auth_system.Controllers.Admin;


// TODO: Move app functions to services.
// TODO: Create a custom class for the services to talk to the controllers.
[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("/api/auth/admin/[controller]")]
public class UsersController(IUsersService usersService) : ApiController
{

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDTO>> GetUser(int id)
    {
        return HandleResult(await usersService.GetUserById(id));
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponseDTO>>> FindUser([FromQuery] UserQueryDTO query)
    {
        return HandleResult(await usersService.FindUsers(query));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDTO>> CreateUser([FromBody] UserDTO userData)
    {
        return HandleResult(await usersService.CreateUser(userData));
    }

    [HttpPost]
    [Route("{id:int}/resend-invitation")]
    public async Task<ActionResult> ResendInvitationEmail(int id)
    {
        return HandleResult(await usersService.ResendInvitationMail(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        return HandleResult(await usersService.DeleteUser(id));
    }
}
