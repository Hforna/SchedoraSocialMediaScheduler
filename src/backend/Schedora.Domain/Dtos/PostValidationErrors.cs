namespace Schedora.Domain.Dtos;

public class PostValidationErrors
{
    public string Platform { get; set; }
    public bool IsValid { get; private set; } = false;
    public string Errors { get; set; }
}