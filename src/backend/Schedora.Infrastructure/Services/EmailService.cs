using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RazorLight;
using Schedora.Domain.Services;
using Schedora.Infrastructure.EmailTemplates.Models;

namespace Schedora.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly RazorLightEngine _razorEngine;
    private readonly SmtpConfigurations _smtpConfigurations;

    public EmailService(IOptions<SmtpConfigurations> smtp)
    {
        _smtpConfigurations = smtp.Value;
        
        var infrastructurePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var templatePath = Path.Combine(infrastructurePath, "EmailTemplates");

        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderEmailConfirmation(string userName, string urlConfirmation, string companyName,
        int expirationHours)
    {
        var model = new ConfirmAccountEmailModel(userName, urlConfirmation, companyName, expirationHours);

        return await _razorEngine.CompileRenderAsync("ConfirmAccountEmail.cshtml", model);
    }

    public async Task SendEmail(string toEmail, string toUserName, string body, string subject)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_smtpConfigurations.UserName, _smtpConfigurations.Email));
        message.To.Add(new MailboxAddress(toUserName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(_smtpConfigurations.Provider, _smtpConfigurations.Port, true);

        await client.AuthenticateAsync(_smtpConfigurations.Email, _smtpConfigurations.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

public class SmtpConfigurations
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public string Provider { get; set; }
}