using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IContentValidatorEngine
{
    public string Platform { get; set; }
    public PostValidationDto Validate(string content);
}