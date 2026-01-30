using System.Runtime.InteropServices.JavaScript;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;

namespace Schedora.Infrastructure.Services.Externals.Validation;

public abstract class ContentValidationHandler
{
    public abstract string Platform { get; set; }
    public List<string> Errors { get; set; } = [];
    private ContentValidationHandler? _nextHandler { get; set; }

    public abstract void Validate(string content);

    public void SetNextHandler(ContentValidationHandler handler)
    {
        _nextHandler = handler;
    }
    
    public void SendNext(string content)
    {
        _nextHandler?.Errors.AddRange(Errors);
        _nextHandler?.Validate(content);
    }
}