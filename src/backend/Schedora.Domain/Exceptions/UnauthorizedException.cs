namespace Schedora.Domain.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string error) : base(error)
    {
    }
}