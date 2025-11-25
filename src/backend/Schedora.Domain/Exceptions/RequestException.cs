namespace Schedora.Domain.Exceptions;

public class RequestException : BaseException
{
    public RequestException(string error) : base(error)
    {
    }
}

public abstract class BaseException : SystemException
{
    public string Error { get; set; }

    protected BaseException(string error) : base(error) {}

    public string GetMessage() => Message;
}