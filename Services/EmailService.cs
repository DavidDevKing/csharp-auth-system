using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService(IOptions<EmailSettings> settingsOptions, IHttpContextAccessor httpContextAccessor, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings settings = settingsOptions.Value;

    public async Task<bool> SendInvitationMail(string recipientName, string recipientEmail, string invitationToken)
    {
        try
        {
            MimeMessage message = new();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));

            message.Subject = "Activate Account";

            HttpRequest? request = httpContextAccessor.HttpContext?.Request;
            string base_url = $"{request?.Scheme}://{request?.Host}";
            string url = $"{base_url}/api/auth/invitations/verify?token={invitationToken}"; // HACK: Supposed to point to a frontend and front end uses this endpoint for token validation

            BodyBuilder bodyBuilder = new()
            {
                HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;">
                    <h2 style="color: #333;">Dear {recipientName},</h2>
                    <p>Your account has been processed. Please click the link below to activate it.</p>
                    <a href="{url}" style="background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;">
                        Activate Account
                    </a>
                    <p style="color : #777; font-style: italic">If this isn't you, or you got this email by accident, you can safely ignore.</p>
                </div>
            """
            };

            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new();

            await client.ConnectAsync(settings.Server, settings.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings.Username, settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send email");
            return false;
        }

        return true;
    }
}