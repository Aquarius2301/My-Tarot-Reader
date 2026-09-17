using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Api.Middlewares;

/// <summary>
/// Catches unhandled exceptions from downstream middleware and converts them
/// into a consistent <see cref="ApiResponse"/> JSON error response.
/// </summary>
public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the next middleware, intercepting <see cref="BaseException"/>,
    /// client cancellation, and any unhandled exception to produce a mapped error response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BaseException appEx)
        {
            await WriteResponseAsync(context, appEx.StatusCode, BuildBody(appEx));
        }
        catch (OperationCanceledException)
        {
            // Client disconnected or aborted the request — not a server fault.
            await WriteResponseAsync(
                context,
                StatusCodes.Status499ClientClosedRequest,
                ApiResponse.Failure(SystemErrorCode.RequestAborted)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred while processing request {Path}",
                context.Request.Path
            );
            await WriteResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiResponse.Failure(SystemErrorCode.InternalServerError)
            );
        }
    }

    /// <summary>
    /// Builds the failure response body for a known application exception,
    /// including field-level errors when present.
    /// </summary>
    /// <param name="appEx">The application exception to convert.</param>
    private static object BuildBody(BaseException appEx)
    {
        return appEx.FieldErrors.Count > 0
            ? ApiResponse.Failure(appEx.ErrorCode, appEx.FieldErrors.ToList())
            : ApiResponse.Failure(appEx.ErrorCode);
    }

    /// <summary>
    /// Writes a JSON error response with the given status code and body.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="statusCode">The HTTP status code to set on the response.</param>
    /// <param name="body">The response body to serialize as JSON.</param>
    private static async Task WriteResponseAsync(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(body);
    }
}
