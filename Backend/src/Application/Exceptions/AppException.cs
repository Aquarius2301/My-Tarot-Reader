using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Exceptions;

/// <summary>
/// Base class for application exceptions that map to a specific HTTP status code
/// and an <see cref="ErrorCode"/>. Framework-agnostic: status codes are plain ints.
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>HTTP status code to return to the client.</summary>
    public int StatusCode { get; }

    /// <summary>Error code (i18n key) returned in the response envelope.</summary>
    public string ErrorCode { get; }

    /// <summary>Optional field-level errors for validation failures.</summary>
    public IReadOnlyList<FieldError> FieldErrors { get; }

    /// <summary>Constructs a base exception with status, error code, message and optional field errors.</summary>
    protected BaseException(
        int statusCode,
        string errorCode,
        string? message = null,
        IReadOnlyList<FieldError>? fieldErrors = null
    )
        : base(message ?? errorCode)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        FieldErrors = fieldErrors ?? Array.Empty<FieldError>();
    }
}

/// <summary>400 - the request was malformed or rejected.</summary>
public class BadRequestException : BaseException
{
    /// <summary>Constructs a bad-request exception.</summary>
    public BadRequestException(string code = ErrorMessageCode.Server.BadRequest)
        : base(400, code) { }
}

/// <summary>400 - one or more fields failed validation.</summary>
public class ValidationException : BaseException
{
    /// <summary>Constructs a validation exception carrying field errors.</summary>
    public ValidationException(
        IReadOnlyList<FieldError> fieldErrors,
        string code = ErrorMessageCode.Server.BadRequest
    )
        : base(400, code, null, fieldErrors) { }
}

/// <summary>401 - authentication missing or invalid.</summary>
public class UnauthorizedException : BaseException
{
    /// <summary>Constructs an unauthorized exception.</summary>
    public UnauthorizedException(string code = ErrorMessageCode.Server.Unauthorized)
        : base(401, code) { }
}

/// <summary>403 - caller lacks permission.</summary>
public class ForbiddenException : BaseException
{
    /// <summary>Constructs a forbidden exception.</summary>
    public ForbiddenException(string code = ErrorMessageCode.Server.Forbidden)
        : base(403, code) { }
}

/// <summary>404 - the requested resource was not found.</summary>
public class NotFoundException : BaseException
{
    /// <summary>Constructs a not-found exception.</summary>
    public NotFoundException(string code = ErrorMessageCode.Server.NotFound)
        : base(404, code) { }
}

/// <summary>409 - the request conflicts with the current state.</summary>
public class ConflictException : BaseException
{
    /// <summary>Constructs a conflict exception.</summary>
    public ConflictException(string code = ErrorMessageCode.Server.Conflict)
        : base(409, code) { }
}

/// <summary>429 - the caller has exceeded a rate limit (e.g. guest already drew today).</summary>
public class TooManyRequestsException : BaseException
{
    /// <summary>Constructs a too-many-requests exception.</summary>
    public TooManyRequestsException(string code = ErrorMessageCode.Tarot.DrawnAlready)
        : base(429, code) { }
}

/// <summary>500 - an unexpected server-side error occurred.</summary>
public class InternalServerException : BaseException
{
    /// <summary>Constructs an internal-server exception.</summary>
    public InternalServerException(
        string code = ErrorMessageCode.Server.InternalServerError,
        string? message = null
    )
        : base(500, code, message) { }
}
