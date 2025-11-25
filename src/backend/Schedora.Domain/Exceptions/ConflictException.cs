namespace Schedora.Domain.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string error) : base(error)
    {
    }
}