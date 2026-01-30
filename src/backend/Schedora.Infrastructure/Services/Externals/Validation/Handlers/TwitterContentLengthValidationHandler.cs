using Microsoft.Extensions.Options;
using Schedora.Domain.Entities;
using Schedora.Infrastructure.Services.Externals.Validation;

namespace Schedora.Infrastructure.Services.Externals;

public class TwitterContentLengthValidationHandler : ContentValidationHandler
{
    private readonly TwitterValidationRules  _validationRules;

    public TwitterContentLengthValidationHandler(IOptions<TwitterValidationRules>  validationRules)
    {
        _validationRules = validationRules.Value;
    }


    public override string Platform { get; set; } = SocialPlatformsNames.Twitter;

    public override void Validate(string content)
    {
        if(string.IsNullOrEmpty(content) && !_validationRules.ContentCanBeNull)
            Errors.Add("Content cannot be null or empty");
        
        if(content.Length >  _validationRules.MaxContentLength)
            Errors.Add($"The max length for content is {_validationRules.MaxContentLength}");
        
        SendNext(content);
    }
}