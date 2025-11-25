namespace Schedora.Domain.Services;

public interface IEmailService
{
    public Task<string> RenderEmailConfirmation(string userName, string urlConfirmation, string companyName,
        int expirationHours);
    public Task SendEmail(string toEmail, string toUserName, string body, string subject);
}