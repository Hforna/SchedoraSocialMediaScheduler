namespace Schedora.Domain.Services;

public interface IEmailService
{
    public Task<string> RenderEmailConfirmation(string userName, string urlConfirmation, string companyName,
        int expirationHours);
    public Task<string> RenderResetPassword(string userName, string companyName, string resetPasswordUrl, int expirationHours);
    public Task<string> RenderSubscriptionActivated(string subscriptionTier, string userName, string companyName);
    public Task SendEmail(string toEmail, string toUserName, string body, string subject);
}