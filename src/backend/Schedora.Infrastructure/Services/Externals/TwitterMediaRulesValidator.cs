using Schedora.Domain.Dtos;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services.Externals;

public class TwitterMediaRulesValidator : IMediaRulesValidator
{
    public void Validate(IEnumerable<MediaDescriptorDto> medias)
    {
        
    }
}

public class TwitterValidationRules
{
    public int MaxImageSizeInMb { get; set; }
    public int MaxWidthFile { get; set; }
    public int MaxHeightFile { get; set; }
    public int MaxVideoSizeInMb { get; set; }
    public int TotalMediasAccepted { get; set; }
    public string Format { get; set; }
}

