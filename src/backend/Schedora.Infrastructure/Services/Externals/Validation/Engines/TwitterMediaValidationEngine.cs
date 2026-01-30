using System.Text;
using Microsoft.Extensions.Options;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services.Externals.Validation.Engines;

public class TwitterMediaValidatorEngine : IMediaValidationEngine
{
    public string Platform { get; set; } =  SocialPlatformsNames.Twitter;
    private readonly IEnumerable<MediaValidationHandler> _handlers;

    public TwitterMediaValidatorEngine(IEnumerable<MediaValidationHandler> handlers)
    {
        _handlers = handlers.Where(d => d.Platform == SocialPlatformsNames.Twitter);
    }
    
    public (bool isValid, string? errors) IsValid(IEnumerable<MediaDescriptorDto> medias)
    {
        bool isValid = true;
        string? errors = null;
        
        if (!medias.Any())
            return (isValid, errors);
        
        var totalHandlers = _handlers.Count();
        if(totalHandlers == 0)
            return (isValid, errors);

        var firstHandler = _handlers.ElementAt(0);
        firstHandler.Errors.Clear();
        for (var i = 0; i < totalHandlers - 1; i++)
        {
            var currHandler = _handlers.ElementAt(i);
            currHandler.NextHandler(_handlers.ElementAt(i + 1));
            _handlers.ElementAt(i + 1).Errors.Clear();
        }
        
        firstHandler.Validate(medias);
        var lastHandler = _handlers.ElementAt(totalHandlers - 1);
        if (lastHandler.Errors.Any())
        {
            isValid = false;
            errors = FormatErrors(lastHandler.Errors);;
        }
        
        return (isValid, errors);
    }

    private string? FormatErrors(List<string> errors)
    {
        var sb = new StringBuilder();
        
        errors.ForEach(e => sb.AppendLine(e));
        
        return sb.ToString();
    }
}