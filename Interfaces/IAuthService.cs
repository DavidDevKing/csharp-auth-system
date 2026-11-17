public interface IAuthService
{
    public Task<ServiceResult> Login(LoginDTO loginDetails);
    public Task<ServiceResult> Logout();
    public Task<ServiceResult> Refresh();
    public Task<ServiceResult<AccessPayloadDTO>> VerifyInvitationToken(TokenStringDTO tokenDTO);
    public Task<ServiceResult> ActivateAccount(AccountActivationDTO activationCreds);
}