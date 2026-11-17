
using Microsoft.AspNetCore.Mvc;
namespace csharp_auth_system.Controllers;

[ApiController]
public class ErrorHandler() : ControllerBase
{
    [Route("/api/error")]
    [HttpGet]
    public async Task<ActionResult> GenericError()
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error" });
    }
}