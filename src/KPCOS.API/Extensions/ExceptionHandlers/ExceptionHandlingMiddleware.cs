using System.Net;
using KPCOS.BusinessLayer.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace KPCOS.API.Extensions.ExceptionHandlers;

public class ExceptionHandlingMiddleware : IExceptionHandler
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails()
        {
            /*StatusCode = context.Response.StatusCode,
            Message = "Internal Server CustomError from the custom middleware."*/
        }.ToString());
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        
        /*var error*/
        return true;
    }
}