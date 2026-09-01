namespace MyTarotReader.Application.Exceptions;

/// <summary>
/// Centralized error codes used across the API response envelope.
/// Nested by category to group related codes and avoid spelling mistakes.
/// Values are the i18n keys sent to the client (e.g. "NotFound").
/// </summary>
public static class ErrorMessageCode
{
    /// <summary>Infrastructure / server-level codes.</summary>
    public static class Server
    {
        /// <summary>Unhandled server error.</summary>
        public const string InternalServerError = "error.system.internalServerError";

        /// <summary>The request was malformed or rejected.</summary>
        public const string BadRequest = "error.system.badRequest";

        /// <summary>Authentication is missing or invalid.</summary>
        public const string Unauthorized = "error.system.unauthorized";

        /// <summary>The caller lacks permission for the resource.</summary>
        public const string Forbidden = "error.system.forbidden";

        /// <summary>The requested resource was not found.</summary>
        public const string NotFound = "error.system.notFound";

        /// <summary>The request conflicts with the current state.</summary>
        public const string Conflict = "error.system.conflict";

        /// <summary>The client closed the connection before the response was sent.</summary>
        public const string RequestAborted = "error.system.requestAborted";
    }

    /// <summary>Tarot feature codes.</summary>
    public static class Tarot
    {
        /// <summary>The guest has already drawn a card today.</summary>
        public const string DrawnAlready = "error.tarot.drawnAlready";

        /// <summary>The card code is invalid.</summary>
        public const string InvalidCard = "error.tarot.invalidCard";

        /// <summary>
        /// The guest key is invalid (e.g. missing, malformed, or expired).
        /// </summary>
        public const string InvalidGuestKey = "error.tarot.invalidGuestKey";
    }

    /// <summary>Authentication feature codes.</summary>
    public static class Auth
    {
        /// <summary>The refresh token is missing, revoked, or does not exist.</summary>
        public const string RefreshTokenInvalid = "error.auth.refreshTokenInvalid";

        /// <summary>The refresh token has expired.</summary>
        public const string RefreshTokenExpired = "error.auth.refreshTokenExpired";

        /// <summary>The request is not authenticated, or the user identity is missing/invalid.</summary>
        public const string Unauthorized = "error.auth.unauthorized";
    }
}
