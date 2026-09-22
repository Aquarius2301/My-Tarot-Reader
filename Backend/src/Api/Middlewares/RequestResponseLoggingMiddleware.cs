using System.Diagnostics;
using System.Text;

namespace MyTarotReader.Api.Middlewares;

/// <summary>
/// Logs each API request and its response to a per-day file under <c>logs/</c>,
/// pairing both entries with a short trace id. Headers and cookies are never logged.
/// </summary>
public class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger
)
{
    private const int MaxBodyLength = 5000;
    private const string TruncationSuffix = "...";
    private const string RequestMarker = "REQ";
    private const string ResponseMarker = "RSP";

    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;

    /// <summary>
    /// Assigns a trace id to the request, logs the incoming call, invokes the rest
    /// of the pipeline, then logs the response under the same trace id.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (context.Request.Method == HttpMethods.Options) // Ignore preflight requests
        {
            await _next(context);
            return;
        }
        var traceId = Guid.NewGuid().ToString("N")[..8];

        var requestBody = await ReadRequestBodyAsync(context.Request);
        await LogAsync(
            traceId,
            RequestMarker,
            stringBuilder =>
                stringBuilder
                    .Append(context.Request.Method)
                    .Append(' ')
                    .Append(context.Request.Path)
                    .Append(context.Request.QueryString)
                    .AppendLine()
                    .Append("Request: ")
                    .AppendLine(requestBody)
        );

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        finally
        {
            await responseBuffer.FlushAsync();
            responseBuffer.Position = 0;

            var responseBody = await ReadResponseBodyAsync(responseBuffer);

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            stopwatch.Stop();
            await LogAsync(
                traceId,
                ResponseMarker,
                stringBuilder =>
                    stringBuilder
                        .Append("-> ")
                        .Append(context.Response.StatusCode)
                        .Append(" (")
                        .Append(stopwatch.ElapsedMilliseconds)
                        .Append(" ms)")
                        .AppendLine()
                        .Append("Response: ")
                        .AppendLine(responseBody)
            );
        }
    }

    /// <summary>
    /// Reads the request body as UTF-8 text without consuming it, so downstream
    /// middleware can still read the stream afterwards.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!HasReadableBody(request.ContentType))
        {
            return "<binary content>";
        }

        request.EnableBuffering();
        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true
        );

        var body = await ReadTruncatedAsync(reader);
        request.Body.Position = 0;
        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(Stream stream)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true
        );

        return await ReadTruncatedAsync(reader);
    }

    /// <summary>
    /// Reads up to <see cref="MaxBodyLength"/> characters and appends
    /// <see cref="TruncationSuffix"/> when the payload is longer.
    /// </summary>
    private static async Task<string> ReadTruncatedAsync(StreamReader reader)
    {
        var buffer = new char[MaxBodyLength + TruncationSuffix.Length];
        var read = await reader.ReadAsync(buffer, 0, buffer.Length);

        var text = new string(buffer, 0, read);
        if (read > MaxBodyLength)
        {
            return text[..MaxBodyLength] + TruncationSuffix;
        }

        return text;
    }

    private static bool HasReadableBody(string? contentType)
    {
        return string.IsNullOrEmpty(contentType)
            || contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("text/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains(
                "application/x-www-form-urlencoded",
                StringComparison.OrdinalIgnoreCase
            );
    }

    private async Task LogAsync(
        string traceId,
        string marker,
        Func<StringBuilder, StringBuilder> messageBuilder
    )
    {
        try
        {
            var line = new StringBuilder()
                .Append('[')
                .Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append("] [")
                .Append(marker)
                .Append(' ')
                .Append(traceId)
                .Append("] ");

            messageBuilder(line);
            line.AppendLine();

            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logsDirectory);

            var filePath = Path.Combine(logsDirectory, $"log_{DateTimeOffset.Now:yyyy-MM-dd}.txt");
            await File.AppendAllTextAsync(filePath, line.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write request/response log");
        }
    }
}
