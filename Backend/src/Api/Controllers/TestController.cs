using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Api.Controllers;

/// <summary>
/// Test controller to exercise the standard API response envelope and global
/// exception handling on Swagger. Not part of the production feature set.
/// </summary>
[ApiController]
[Route("api/test")]
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
    public IActionResult BadResult() =>
        throw new BadRequestException(ErrorMessageCode.Server.BadRequest);

    /// <summary>Throws a 400 validation exception carrying field-level errors.</summary>
    [HttpGet("validation")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidationResult() =>
        throw new ValidationException(
            new List<FieldError>
            {
                new("password", ErrorMessageCode.Server.BadRequest),
                new("email", ErrorMessageCode.Server.BadRequest),
            }
        );

    /// <summary>Throws an unexpected exception, mapped to a 500 InternalServerError envelope.</summary>
    [HttpGet("boom")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Boom() =>
        throw new InvalidOperationException("Something unexpected happened");
}
