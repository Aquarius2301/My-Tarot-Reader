using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Common.Exceptions;

/// <summary>
/// Base class for application exceptions that map to a specific HTTP status code
/// and an <see cref="ErrorCode"/>. Framework-agnostic: status codes are plain ints.
/// </summary>
/// <remarks>Constructs a base exception with status, error code, message and optional field errors.</remarks>
public abstract class BaseException(
    int statusCode,
    string errorCode,
    string? message = null,
    IReadOnlyList<FieldError>? fieldErrors = null,
    Exception? innerException = null
) : Exception(message ?? errorCode, innerException)
{
    /// <summary>HTTP status code to return to the client.</summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>Error code (i18n key) returned in the response envelope.</summary>
    public string ErrorCode { get; } = errorCode;

    /// <summary>Optional field-level errors for validation failures.</summary>
    public IReadOnlyList<FieldError> FieldErrors { get; } = fieldErrors ?? [];
}

/// <summary>400 - the request was malformed or rejected.</summary>
public class BadRequestException(string code = SystemErrorCode.BadRequest)
    : BaseException(400, code) { }

/// <summary>400 - one or more fields failed validation.</summary>
public class ValidationException(
    IReadOnlyList<FieldError> fieldErrors,
    string code = SystemErrorCode.BadRequest
) : BaseException(400, code, null, fieldErrors) { }

/// <summary>401 - authentication missing or invalid.</summary>
/// <remarks>Constructs an unauthorized exception.</remarks>
public class UnauthorizedException(string code = SystemErrorCode.Unauthorized)
    : BaseException(401, code) { }

/// <summary>403 - caller lacks permission.</summary>
public class ForbiddenException(string code = SystemErrorCode.Forbidden)
    : BaseException(403, code) { }

/// <summary>404 - the requested resource was not found.</summary>
/// <remarks>Constructs a not-found exception.</remarks>
public class NotFoundException(string code = SystemErrorCode.NotFound)
    : BaseException(404, code) { }

/// <summary>409 - the request conflicts with the current state.</summary>
/// <remarks>Constructs a conflict exception.</remarks>
public class ConflictException(string code = SystemErrorCode.Conflict)
    : BaseException(409, code) { }

/// <summary>429 - the caller has exceeded a rate limit (e.g. guest already drew today).</summary>
public class TooManyRequestsException(string code = SystemErrorCode.TooManyRequests)
    : BaseException(429, code) { }

/// <summary>500 - an unexpected server-side error occurred.</summary>
public class InternalServerException(
    string code = SystemErrorCode.InternalServerError,
    string? message = null,
    Exception? innerException = null
) : BaseException(500, code, message, null, innerException) { }
