using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using KPCOS.Common;
using System.Net;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace KPCOS.WebFramework.Middlewares
{
    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }

    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next,
            IHostingEnvironment env,
            ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }
        private async Task HandleAppExceptionAsync(HttpContext context, AppException exception)
        {
            _logger.LogError(exception, exception.Message);
            context.Response.StatusCode = (int)exception.HttpStatusCode;
            var result = new ApiResult(false, exception.ApiStatusCode, exception.Message);
            await WriteToResponseAsync(context, result);
        }

        private async Task HandleSecurityTokenExpiredExceptionAsync(HttpContext context, SecurityTokenExpiredException exception)
        {
            _logger.LogError(exception, exception.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var result = new ApiResult(false, ApiResultStatusCode.UnAuthorized, "Token has expired.");
            await WriteToResponseAsync(context, result);
        }

        private async Task HandleUnauthorizedAccessExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
        {
            _logger.LogError(exception, exception.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var result = new ApiResult(false, ApiResultStatusCode.UnAuthorized, "Access denied. You are not authorized to access this resource.");
            await WriteToResponseAsync(context, result);
        }

        private async Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var result = new ApiResult(false, ApiResultStatusCode.ServerError, "An error occurred while processing your request.");
            await WriteToResponseAsync(context, result);
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException exception)
            {
                await HandleAppExceptionAsync(context, exception);
            }
            catch (SecurityTokenExpiredException exception)
            {
                await HandleSecurityTokenExpiredExceptionAsync(context, exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                await HandleUnauthorizedAccessExceptionAsync(context, exception);
            }
            catch (Exception exception)
            {
                await HandleGenericExceptionAsync(context, exception);
            }
        }
        
        private async Task WriteToResponseAsync(HttpContext context, ApiResult result)
        {
            context.Response.ContentType = "application/json";
            var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
            
            await context.Response.WriteAsync(json);
        }
    }
}
