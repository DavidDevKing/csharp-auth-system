
public interface IUsersService
{
    public Task<ServiceResult<UserResponseDTO>> GetUserById(int id);
    public Task<ServiceResult<List<UserResponseDTO>>> FindUsers(UserQueryDTO query);
    public Task<ServiceResult<UserResponseDTO>> CreateUser(UserDTO userData);
    public Task<ServiceResult> ResendInvitationMail(int id);
    public Task<ServiceResult> DeleteUser(int id);
}