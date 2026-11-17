public interface IEmailService
{
    public Task<bool> SendInvitationMail(string recipientName, string recipientEmail, string invitationToken);
}