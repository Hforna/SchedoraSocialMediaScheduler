namespace Schedora.Domain.Dtos;

public class PostValidationDto
{
    public required string Platform { get; set; }
    public bool IsValid { get; set; }
    public string? Errors { get; set; }
}