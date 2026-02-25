namespace Schedora.Domain.Dtos;

public sealed class SocialCreatePostRequestDto
{
    public string? Text { get; init; }
    public List<string> MediaIds { get; init; } = new();
}
