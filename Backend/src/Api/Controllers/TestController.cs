using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Api.Controllers;

/// <summary>
/// Test controller to exercise the standard API response envelope and global
/// exception handling on Swagger. Development-only; never mapped in production.
/// </summary>
[ApiController]
[Route("api/test")]
[DevelopmentOnly]
public class TestController : ControllerBase
{
    /// <summary>Returns a success envelope with a sample payload.</summary>
    [HttpGet("ok")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult OkResult() =>
        Ok(ApiResponse.Success(new { message = "Everything is fine" }));

    /// <summary>Throws a 404 NotFound exception, mapped to a failure envelope.</summary>
    [HttpGet("not-found")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult NotFoundResult() => throw new NotFoundException();

    /// <summary>Throws a 400 bad-request exception with a specific error code.</summary>
    [HttpGet("bad")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BadResult() => throw new BadRequestException();

    /// <summary>Throws a 400 validation exception carrying field-level errors.</summary>
    [HttpGet("validation")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidationResult() =>
        throw new ValidationException([
            new("password", SystemErrorCode.BadRequest),
            new("email", SystemErrorCode.BadRequest),
        ]);

    /// <summary>Throws an unexpected exception, mapped to a 500 InternalServerError envelope.</summary>
    [HttpGet("boom")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Boom() =>
        throw new InvalidOperationException("Something unexpected happened");
}
