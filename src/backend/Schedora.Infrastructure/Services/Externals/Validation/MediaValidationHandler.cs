using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;

namespace Schedora.Infrastructure.Services.Externals.Validation;

public abstract class MediaValidationHandler
{
    public abstract string Platform { get; set; }
    public List<string> Errors { get; set; } = [];
    private MediaValidationHandler? _nextHandler;
    public void SetPlatform(string platform)
    {
        Platform = SocialPlatformsNames.GetAllConsts().Contains(platform) 
            ?  platform 
            : throw new DomainException("Invalid social platform name");;
    }

    public abstract void Validate(IEnumerable<MediaDescriptorDto> dto);

    public void NextHandler(MediaValidationHandler handler)
    {
        _nextHandler = handler;
    }

    protected void SendNext(IEnumerable<MediaDescriptorDto> dto)
    {
        _nextHandler?.Errors.AddRange(Errors);
        _nextHandler?.Validate(dto);
    }
}