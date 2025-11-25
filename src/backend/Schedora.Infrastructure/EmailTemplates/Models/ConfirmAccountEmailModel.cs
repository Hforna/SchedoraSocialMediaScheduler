namespace Schedora.Infrastructure.EmailTemplates.Models;

public class ConfirmAccountEmailModel
{
    public ConfirmAccountEmailModel(string userName, string confirmationUrl, string companyName, int expirationHours)
    {
        UserName = userName;
        ConfirmationUrl = confirmationUrl;
        CompanyName = companyName;
        ExpirationHours = expirationHours;
    }

    public string UserName { get; set; }
    public string ConfirmationUrl { get; set; }
    public string CompanyName { get; set; }
    public int ExpirationHours { get; set; } = 24;
}
