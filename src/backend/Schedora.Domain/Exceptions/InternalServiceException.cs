namespace Schedora.Domain.Exceptions;

public class InternalServiceException : BaseException
{
    public InternalServiceException(string error) : base(error)
    {
    }
}