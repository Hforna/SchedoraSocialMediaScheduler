using System.Text;
using Microsoft.Extensions.Options;
using Schedora.Application.Services;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Services.Externals.Validation;
using Stripe.Events;

namespace Schedora.Infrastructure.Services.Externals;

public class TwitterMediaValidatorEngine : IMediaRulesValidator
{
    private readonly TwitterValidationRules _rules;
    private readonly IEnumerable<MediaValidationHandler> _handlers;

    public TwitterMediaValidatorEngine(IOptions<TwitterValidationRules> rules,  IEnumerable<MediaValidationHandler> handlers)
    {
        _rules = rules.Value;
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

public class TwitterMediaDimensionsValidationHandler : MediaValidationHandler
{
    private readonly TwitterValidationRules _rules;

    public TwitterMediaDimensionsValidationHandler(TwitterValidationRules rules)
    {
        _rules = rules;
    }

    public override string Platform { get; set; } = SocialPlatformsNames.Twitter;

    public override void Validate(IEnumerable<MediaDescriptorDto> dto)
    {
        var maxHeight = dto.OrderByDescending(d => d.Height).FirstOrDefault().Height;
        if(maxHeight > _rules.MaxHeightFile)
            Errors.Add("- The max height for file is: " + _rules.MaxHeightFile);

        SendNext(dto);
    }
}

public class TwitterMediaSizeValidationHandler : MediaValidationHandler
{
    public override string Platform { get; set; } = SocialPlatformsNames.Twitter;
    private readonly TwitterValidationRules _rules;

    public TwitterMediaSizeValidationHandler(TwitterValidationRules rules)
    {
        _rules = rules;
    }

    public override void Validate(IEnumerable<MediaDescriptorDto> dto)
    {
        var maxVideoSize = dto.OrderByDescending(d => d.SizeInMb)
            .FirstOrDefault(d => d.Type == MediaType.VIDEO);
        var maxImageSize = dto.OrderByDescending(d => d.SizeInMb).FirstOrDefault(d => d.Type == MediaType.IMAGE);
        
        if (maxVideoSize != null && maxVideoSize.SizeInMb > _rules.MaxVideoSizeInMb)
            Errors.Add($"- The max media size for video type is {_rules.MaxVideoSizeInMb}MB");
        if (maxImageSize != null && maxImageSize.SizeInMb > _rules.MaxImageSizeInMb)
        {
            Errors.Add($"- The max media size image type is {_rules.MaxImageSizeInMb}MB");
        }
        
        SendNext(dto);
    }
}

public class TwitterMaxMediaValidationHandler : MediaValidationHandler
{
    public override string Platform { get; set; } = SocialPlatformsNames.Twitter;
    private readonly TwitterValidationRules _rules;

    public TwitterMaxMediaValidationHandler(TwitterValidationRules rules)
    {
        _rules = rules;
    }

    public override void Validate(IEnumerable<MediaDescriptorDto> dto)
    {
        if(dto.Count() > _rules.TotalMediasAccepted)
            Errors.Add($"- The media limits per post is up to: {_rules.TotalMediasAccepted}");
        
        SendNext(dto);
    }
}

public class TwitterFormatsValidationHandler : MediaValidationHandler
{
    public override string Platform { get; set; } = SocialPlatformsNames.Twitter;
    private readonly TwitterValidationRules _rules;
    
    public TwitterFormatsValidationHandler(TwitterValidationRules rules) => _rules = rules;
    
    public override void Validate(IEnumerable<MediaDescriptorDto> dto)
    {
        if(dto.Any(d => !_rules.Formats.Contains(d.Format)))
            Errors.Add($"- The only media formats supported are: {_rules.Formats}");
        
        SendNext(dto);
    }
}

public class TwitterValidationRules
{
    public int MaxImageSizeInMb { get; set; }
    public int MaxWidthFile { get; set; }
    public int MaxHeightFile { get; set; }
    public int MaxVideoSizeInMb { get; set; }
    public int TotalMediasAccepted { get; set; }
    public List<string> Formats { get; set; }
}

