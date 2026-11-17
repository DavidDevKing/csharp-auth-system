
using Microsoft.AspNetCore.Mvc;
namespace csharp_auth_system.Controllers;

[ApiController]
[Route("/api/auth/refresh")]
public class RefreshController(IAuthService authService) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> RefreshTokens()
    {
        return HandleResult(await authService.Refresh());
    }
}