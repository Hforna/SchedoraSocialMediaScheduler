using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Schedora.Domain.Exceptions;

namespace Schedora.WebApi.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var error = exception.Message;

        if (exception is BaseException be)
        {
            error = be.GetMessage();
            statusCode = be switch
            {
                DomainException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                RequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                ExternalServiceException => StatusCodes.Status500InternalServerError,
                InternalServiceException => StatusCodes.Status500InternalServerError,
                _ => statusCode
            };
        }
        else
        {
            error = "An unexpected error has occured";
        }
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Type = exception.GetType().Name,
            Status = statusCode,
            Title = "An error has occured",
            Detail = error
        },  cancellationToken);

        return true;
    }
}