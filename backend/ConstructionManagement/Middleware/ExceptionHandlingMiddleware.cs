using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ConstructionManagement.Middleware
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error occurred while processing request {Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, "VALIDATION_ERROR", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for request {Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "UNAUTHORIZED", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred.");
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string code, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                code,
                message
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
