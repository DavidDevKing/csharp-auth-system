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
    public async Task<IActionResult> GetUser(int id)
    {
        return HandleResult(await usersService.GetUserById(id));
    }

    [HttpGet]
    public async Task<IActionResult> FindUser([FromQuery] UserQueryDTO query)
    {
        return HandleResult(await usersService.FindUsers(query));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDTO userData)
    {
        return HandleResult(await usersService.CreateUser(userData));
    }

    [HttpPost]
    [Route("{id:int}/resend-invitation")]
    public async Task<IActionResult> ResendInvitationEmail(int id)
    {
        return HandleResult(await usersService.ResendInvitationMail(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        return HandleResult(await usersService.DeleteUser(id));
    }
}
