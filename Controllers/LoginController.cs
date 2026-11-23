using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace csharp_auth_system.Controllers;

[ApiController]
public class LoginController(IAuthService authService) : ApiController
{
    [HttpPost]
    [Route("api/auth/login")]
    public async Task<ActionResult> Login([FromBody] LoginDTO loginDetails)
    {
        return HandleResult(await authService.Login(loginDetails));
    }



    [Authorize]
    [HttpPost]
    [Route("/api/auth/logout")]
    public async Task<ActionResult> Logout()
    {
        return HandleResult(await authService.Logout());
    }
}
