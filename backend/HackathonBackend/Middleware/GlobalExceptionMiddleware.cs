using System.Net;
using System.Text.Json;

namespace HackathonBackend.Middleware
{
    /// <summary>
    /// Catches any unhandled exception in the request pipeline and converts it
    /// to a consistent JSON envelope so the Angular frontend always sees the
    /// same error shape: { status, title, detail }.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = new
                {
                    status = 500,
                    title = "Internal Server Error",
                    detail = "An unexpected error occurred. Please try again.",
                    message = "An unexpected error occurred. Please try again."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}
