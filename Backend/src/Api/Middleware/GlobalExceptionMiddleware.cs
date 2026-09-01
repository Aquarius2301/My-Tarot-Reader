using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Api.Middleware;

/// <summary>
/// Catches exceptions thrown anywhere in the pipeline and converts them into the
/// uniform <see cref="ApiResponse{T}"/> envelope with the appropriate HTTP status code.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>Constructs the middleware.</summary>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the next middleware, intercepting any thrown exception.</summary>
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
                ApiResponse.Failure(ErrorMessageCode.Server.RequestAborted)
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
                ApiResponse.Failure(ErrorMessageCode.Server.InternalServerError)
            );
        }
    }

    private static object BuildBody(BaseException appEx)
    {
        return appEx.FieldErrors.Count > 0
            ? ApiResponse.Failure(appEx.ErrorCode, appEx.FieldErrors.ToList())
            : ApiResponse.Failure(appEx.ErrorCode);
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(body);
    }
}
