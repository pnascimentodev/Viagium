using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Viagium.Configurations;
using Viagium.EntitiesDTO.Email;
using Viagium.Services.Interfaces;
using MailKit.Security;
using System.Text;

namespace Viagium.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(SendEmailDTO dto)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(dto.To));
        // Define o assunto normalmente, MimeKit já trata UTF-8
        message.Subject = dto.Subject;
        message.Headers.Replace("Content-Type", "text/html; charset=UTF-8");

        var builder = new BodyBuilder { HtmlBody = dto.HtmlBody };
        builder.HtmlBody = dto.HtmlBody;
        builder.TextBody = null;
        builder.Attachments.Clear();
        // Garante que o charset do corpo é UTF-8
        builder.HtmlBody = dto.HtmlBody;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
