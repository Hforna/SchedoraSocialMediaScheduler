namespace Schedora.Domain.Exceptions;

public class ExternalServiceException : BaseException
{
    public ExternalServiceException(string error) : base(error)
    {
    }
}