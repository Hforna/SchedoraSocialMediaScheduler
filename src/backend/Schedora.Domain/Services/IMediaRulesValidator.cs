using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IMediaRulesValidator
{
    public (bool isValid, string? errors) IsValid(IEnumerable<MediaDescriptorDto> medias);
}