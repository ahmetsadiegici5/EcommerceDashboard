using FluentValidation;
using Grpc.Core;

namespace EcommerceAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred. Path={Path}, Method={Method}", 
                    httpContext.Request.Path, httpContext.Request.Method);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var (statusCode, message, errors) = exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "Doğrulama hatası.",
                    validationEx.Errors.Select(e => e.ErrorMessage).ToList()
                ),
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    exception.Message,
                    (List<string>?)null
                ),
                ArgumentNullException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    (List<string>?)null
                ),
                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    (List<string>?)null
                ),
                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    (List<string>?)null
                ),
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    exception.Message,
                    (List<string>?)null
                ),
                RpcException rpcEx => (
                    StatusCodes.Status503ServiceUnavailable,
                    "Veritabanı servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.",
                    (List<string>?)null
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Beklenmeyen bir hata meydana geldi. Lütfen daha sonra tekrar deneyin.",
                    (List<string>?)null
                )
            };

            context.Response.StatusCode = statusCode;

            var response = new Dictionary<string, object>
            {
                { "statusCode", statusCode },
                { "message", message }
            };

            if (errors != null && errors.Any())
            {
                response["errors"] = errors;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
