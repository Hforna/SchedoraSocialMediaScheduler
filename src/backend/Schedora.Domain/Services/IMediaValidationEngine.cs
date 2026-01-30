using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IMediaValidationEngine
{
    public (bool isValid, string? errors) IsValid(IEnumerable<MediaDescriptorDto> medias);
    public string Platform { get; set; }
}