namespace MyTarotReader.Application.Common.Models;

/// <summary>
/// Represents a field error with a key and value.
/// </summary>
/// <param name="Key">The key of the field error (e.g., email).</param>
/// <param name="Value">The value of the field error (e.g., "The email is already in use").</param>
public record FieldError(string Key, string Value);

/// <summary>
/// Represents a standardized API response with a success status, message, and optional data.
/// </summary>
/// <typeparam name="T">The type of the data in the response.</typeparam>
/// <param name="Success">The success status of the response.</param>
/// <param name="Message">The message of the response.</param>
/// <param name="Data">The data of the response.</param>
public record ApiResponse<T>(bool Success, string? Message = null, T? Data = default);

/// <summary>
/// Provides static methods to create standardized API responses for success and failure scenarios.
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// Creates a successful API response with an optional message and no data.
    /// </summary>
    public static ApiResponse<object> Success(string? message = null) => new(true, message, null);

    /// <summary>
    /// Creates a successful API response with the specified data and an optional message.
    /// </summary>
    public static ApiResponse<T> Success<T>(T data, string? message = null) =>
        new(true, message, data);

    /// <summary>
    /// Creates a failed API response with an optional message and no data.
    /// </summary>
    public static ApiResponse<object> Failure(string? message = null) => new(false, message, null);

    /// <summary>
    /// Creates a failed API response with the specified message and optional data.
    /// </summary>
    public static ApiResponse<T> Failure<T>(string? message, T? errors = default) =>
        new(false, message, errors);
}
