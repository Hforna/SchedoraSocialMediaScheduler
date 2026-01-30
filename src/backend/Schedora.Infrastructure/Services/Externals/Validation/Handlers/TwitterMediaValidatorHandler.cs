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

public class TwitterMediaDimensionsValidationHandler : MediaValidationHandler
{
    private readonly TwitterValidationRules _rules;

    public TwitterMediaDimensionsValidationHandler(IOptions<TwitterValidationRules> rules)
    {
        _rules = rules.Value;
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

    public TwitterMediaSizeValidationHandler(IOptions<TwitterValidationRules> rules)
    {
        _rules = rules.Value;
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

    public TwitterMaxMediaValidationHandler(IOptions<TwitterValidationRules> rules)
    {
        _rules = rules.Value;
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
    
    public TwitterFormatsValidationHandler(IOptions<TwitterValidationRules> rules) => _rules = rules.Value;
    
    public override void Validate(IEnumerable<MediaDescriptorDto> dto)
    {
        if(dto.Any(d => !_rules.GetFormats().Contains(d.Format)))
            Errors.Add($"- The only media formats supported are: {_rules.GetFormats()}");
        
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
    public List<string> Formats { private get; set; } = [];
    public int MaxContentLength { get; set; }
    public bool ContentCanBeNull { get; set; } 
    
    public string GetFormats() =>  string.Join(", ", Formats);
}

