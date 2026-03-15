using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace FlowDesk.Infrastructure.Services;

public class EmailService(IConfiguration config)
{
    public async Task SendInviteAsync(
        string toEmail, string workspaceName, string inviteUrl)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(config["Mail:From"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"You're invited to {workspaceName} on FlowDesk";
        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>You've been invited to join <strong>{workspaceName}</strong></h2>
                <p>Click below to accept - link expires in 7 days.</p>
                <a href="{inviteUrl}"
                   style="background:#1D4ED8;color:white;padding:12px 24px;
                          border-radius:8px;text-decoration:none;
                          display:inline-block;font-family:sans-serif;">
                   Accept Invitation
                </a>
                """
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(config["Mail:Host"],
            int.Parse(config["Mail:Port"]!));
        await smtp.AuthenticateAsync(
            config["Mail:Username"], config["Mail:Password"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
