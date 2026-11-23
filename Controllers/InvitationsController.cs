
using Microsoft.AspNetCore.Mvc;
namespace csharp_auth_system.Controllers;

[ApiController]
public class InvitationsController(IAuthService authService) : ApiController
{
    [Route("/api/auth/invitations/verify")]
    [HttpPost]
    public async Task<ActionResult<AccessPayloadDTO>> VerifyInvitationToken([FromBody] TokenStringDTO tokenDTO)
    {
        return HandleResult(await authService.VerifyInvitationToken(tokenDTO));
    }

    [Route("/api/auth/invitations/accept")]
    [HttpPost]
    public async Task<ActionResult> ActivateAccount([FromBody] AccountActivationDTO activationCreds)
    {
        return HandleResult(await authService.ActivateAccount(activationCreds));
    }
}