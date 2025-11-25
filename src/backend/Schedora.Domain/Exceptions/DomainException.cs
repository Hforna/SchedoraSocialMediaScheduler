namespace Schedora.Domain.Exceptions;

public class DomainException : BaseException
{
    public DomainException(string error) : base(error)
    {
    }
}