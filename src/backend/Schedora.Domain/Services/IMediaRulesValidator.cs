using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IMediaRulesValidator
{
    public void Validate(IEnumerable<MediaDescriptorDto> medias);
}