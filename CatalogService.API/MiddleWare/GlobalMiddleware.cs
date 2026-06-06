using System.Net;
using System.Text.Json;
using Serilog;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // continue pipeline
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception occurred");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        try
        {
            
            response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                message = exception.Message,
                statusCode = response.StatusCode
            });

            return response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception occurred");
        }
        return response.WriteAsync(JsonSerializer.Serialize(new{success = false}));
        
    }
}