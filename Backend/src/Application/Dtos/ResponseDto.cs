namespace MyTarotReader.Application.Dtos;

/// <summary>
/// A single field-level validation error: the field name and its error code.
/// </summary>
/// <param name="Key">The field/property name (e.g. "password").</param>
/// <param name="Value">The error code for that field (e.g. "InvalidPassword").</param>
public record FieldError(string Key, string Value);

/// <summary>
/// Uniform API response envelope. Every endpoint returns <c>{ success, message, data }</c>.
/// On success <see cref="Message"/> is null and <see cref="Data"/> holds the payload (which
/// may be null — use the parameterless <see cref="ApiResponse.Success()"/> for no payload).
/// On failure <see cref="Message"/> carries an error code and <see cref="Data"/> is null
/// (or a list of <see cref="FieldError"/> for validation failures).
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public class ApiResponse<T>
{
    /// <summary>True when the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Error code on failure, null on success.</summary>
    public string? Message { get; set; }

    /// <summary>Response payload; null on failure (or a field-error list for validation).</summary>
    public T? Data { get; set; }
}

/// <summary>
/// Factory helpers for building <see cref="ApiResponse{T}"/> envelopes.
/// </summary>
public static class ApiResponse
{
    /// <summary>Build a success envelope with no payload (Data is null).</summary>
    public static ApiResponse<object?> Success() => new() { Success = true, Data = null };

    /// <summary>Build a success envelope with the given payload.</summary>
    public static ApiResponse<T?> Success<T>(T? data) => new() { Success = true, Data = data };

    /// <summary>Build a failure envelope with just an error code.</summary>
    public static ApiResponse<object?> Failure(string code) =>
        new() { Success = false, Message = code };

    /// <summary>Build a failure envelope carrying field-level validation errors.</summary>
    public static ApiResponse<List<FieldError>> Failure(string code, List<FieldError> errors) =>
        new()
        {
            Success = false,
            Message = code,
            Data = errors,
        };
}
