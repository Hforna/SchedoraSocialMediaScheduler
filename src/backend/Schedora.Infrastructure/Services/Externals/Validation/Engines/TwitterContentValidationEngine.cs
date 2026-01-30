using System.Text;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services.Externals.Validation.Engines;

public class TwitterContentValidationEngine : IContentValidatorEngine
{
    private readonly IEnumerable<ContentValidationHandler>  _contentValidationHandlers;

    public TwitterContentValidationEngine(IEnumerable<ContentValidationHandler> contentValidationHandlers)
    {
        _contentValidationHandlers = contentValidationHandlers.Where(d => d.Platform == SocialPlatformsNames.Twitter);
    }

    public string Platform { get; set; } = SocialPlatformsNames.Twitter;

    public PostValidationResponseDto Validate(string content)
    {
        var response = new PostValidationResponseDto()
        {
            IsValid = true,
            Errors = null,
            Platform = SocialPlatformsNames.Twitter
        };

        if (!_contentValidationHandlers.Any())
            return response;

        var firstHandler = _contentValidationHandlers.First();
        firstHandler.Errors.Clear();

        for (var i = 0; i < _contentValidationHandlers.Count() - 1; i++)
        {
            var currHandler = _contentValidationHandlers.ElementAt(i);
            currHandler.SetNextHandler(_contentValidationHandlers.ElementAt(i + 1));
            _contentValidationHandlers.ElementAt(i + 1).Errors.Clear();
        }
        
        firstHandler.Validate(content);
        var lastHandler = _contentValidationHandlers.Last();

        if (lastHandler.Errors.Count == 0) return response;
        
        var formatedErrors = FormatErrors(lastHandler.Errors);
        response.Errors = formatedErrors;
        response.IsValid = false;

        return response;
    }

    private string FormatErrors(List<string> errors)
    {
        var sb = new StringBuilder();
        
        errors.ForEach(d => sb.AppendLine(d));
        
        return sb.ToString();
    }
}