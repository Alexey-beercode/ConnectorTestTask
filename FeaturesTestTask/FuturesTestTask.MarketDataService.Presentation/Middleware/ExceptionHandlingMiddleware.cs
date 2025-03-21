using System.Net;

namespace FuturesTestTask.Presentation.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Произошла ошибка: {Message}", exception.Message);
            
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            context.Items["Exception"] = exception;
            
            context.Response.Redirect("/Home/Error");

            return Task.CompletedTask;
        }
    }
}