using Newtonsoft.Json;
using Pandora.Admin.WebAPI.Exceptions;
using Pandora.Admin.WebAPI.Models;
using System.Net;

namespace Pandora.Admin.WebAPI.Middlewares
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment webHostEnvironment)
        {
            _next = next;
            this.logger = loggerFactory.CreateLogger(GetType().FullName);
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Func<string, HttpStatusCode, string, Task> func = async (exceptionCode, httpStatusCode, message) =>
            {
                logger.LogError(exception, $"Global Exception Handling :{exceptionCode}");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatusCode;
                context.Response.OnStarting(() =>
                {
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new MessageModel
                    {
                        Code = exceptionCode,
                        Message = message
                    }));
                });
            };

            switch (exception)
            {
                case BusinessException _:
                    await func("BUSINESS_EXCEPTION", HttpStatusCode.BadRequest, exception?.Message);
                    break;
                case ArgumentNullException _:
                case ArgumentException _:
                    await func("ARGUMENT_EXCEPTION", HttpStatusCode.BadRequest, exception?.Message);
                    break;
                case UnauthorizedAccessException _:
                    await func("AUTHENTICATION_EXCEPTION", HttpStatusCode.Unauthorized, exception?.Message);
                    break;
                default:
                    if (webHostEnvironment.IsDevelopment() || webHostEnvironment.IsStaging())
                    {
                        await func("SYSTEM_EXCEPTION", HttpStatusCode.BadRequest, exception?.Message);
                    }
                    else
                    {
                        await func("SYSTEM_EXCEPTION", HttpStatusCode.BadRequest, "Error");
                    }
                    break;
            }
        }
    }

    public static class ExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandler>();
        }
    }
}
