using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;
using MyTarotReader.Infrastructure.Common;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="GeminiClient"/> fallback behavior, exercised through a
/// stubbed <see cref="HttpMessageHandler"/> so no real network call is made.
/// </summary>
public class GeminiClientTests
{
    private const string Prompt = "Interpret this spread";

    #region Helpers

    private static HttpResponseMessage JsonSuccess(string text) =>
        new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(
                new
                {
                    candidates = new[]
                    {
                        new
                        {
                            content = new { parts = new[] { new { text } } },
                        },
                    },
                }
            ),
        };

    private static HttpResponseMessage JsonEmpty() =>
        new() { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new { }) };

    private static HttpResponseMessage Error(HttpStatusCode status) =>
        new() { StatusCode = status };

    private static GeminiClient CreateSut(
        List<GeminiApiSetting> apis,
        Func<HttpRequestMessage, HttpResponseMessage> handler
    )
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler));
        return new GeminiClient(httpClient, Options.Create(new GeminiSetting { Apis = apis }));
    }

    private static List<GeminiApiSetting> Credentials(params string[] models) =>
        models
            .Select((m, i) => new GeminiApiSetting { ApiKey = $"key-{i}", Model = m })
            .ToList();

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> handler
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(handler(request));
    }

    #endregion

    [Fact]
    public async Task GenerateContentAsync_FirstApiSucceeds_ReturnsText()
    {
        // Arrange
        var sut = CreateSut(
            Credentials("gemini-2.0-flash"),
            _ => JsonSuccess("The Lovers: partnership")
        );

        // Act
        var result = await sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Lovers: partnership");
    }

    [Fact]
    public async Task GenerateContentAsync_FirstApi503_FallsBackToSecond()
    {
        // Arrange
        var requests = new List<string>();
        var sut = CreateSut(
            Credentials("gemini-2.0-flash", "gemini-2.5-flash"),
            request =>
            {
                requests.Add(request.RequestUri!.Segments[^1].Replace(":generateContent", ""));
                return requests.Count == 1 ? Error(HttpStatusCode.ServiceUnavailable) : JsonSuccess("The Sun");
            }
        );

        // Act
        var result = await sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Sun");
        requests.Should().BeEquivalentTo(["gemini-2.0-flash", "gemini-2.5-flash"]);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task GenerateContentAsync_ErrorStatus_FallsBackToSecond(HttpStatusCode status)
    {
        // Arrange
        var first = true;
        var sut = CreateSut(
            Credentials("gemini-2.0-flash", "gemini-2.5-flash"),
            _ =>
            {
                if (first)
                {
                    first = false;
                    return Error(status);
                }

                return JsonSuccess("The Star");
            }
        );

        // Act
        var result = await sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Star");
    }

    [Fact]
    public async Task GenerateContentAsync_FirstApiReturnsEmptyContent_FallsBackToSecond()
    {
        // Arrange
        var first = true;
        var sut = CreateSut(
            Credentials("gemini-2.0-flash", "gemini-2.5-flash"),
            _ =>
            {
                if (first)
                {
                    first = false;
                    return JsonEmpty();
                }

                return JsonSuccess("The Moon");
            }
        );

        // Act
        var result = await sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Moon");
    }

    [Fact]
    public async Task GenerateContentAsync_AllApisFail_ThrowsInternalServerException()
    {
        // Arrange
        var sut = CreateSut(
            Credentials("gemini-2.0-flash", "gemini-2.5-flash"),
            _ => Error(HttpStatusCode.ServiceUnavailable)
        );

        // Act
        var act = () => sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task GenerateContentAsync_SingleApiFails_ThrowsInternalServerException()
    {
        // Arrange
        var sut = CreateSut(Credentials("gemini-2.0-flash"), _ => Error(HttpStatusCode.BadRequest));

        // Act
        var act = () => sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task GenerateContentAsync_NoCredentials_ThrowsInternalServerException()
    {
        // Arrange
        var sut = CreateSut([], _ => JsonSuccess("unreachable"));

        // Act
        var act = () => sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Theory]
    [InlineData("", "model-1")]
    [InlineData("key", "")]
    public async Task GenerateContentAsync_InvalidCredentials_Ignored(string apiKey, string model)
    {
        // Arrange
        var sut = CreateSut(
            [new GeminiApiSetting { ApiKey = apiKey, Model = model }],
            _ => JsonSuccess("unreachable")
        );

        // Act
        var act = () => sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task GenerateContentAsync_FourApisConfigured_OnlyTriesThree()
    {
        // Arrange
        var attempts = 0;
        var sut = CreateSut(
            Credentials("model-1", "model-2", "model-3", "model-4"),
            _ =>
            {
                attempts++;
                return Error(HttpStatusCode.ServiceUnavailable);
            }
        );

        // Act
        var act = () => sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task GenerateContentAsync_ManyCalls_UsesAllApisAsStart()
    {
        // Arrange
        var models = new[] { "model-1", "model-2", "model-3", "model-4", "model-5" };
        var requestedModels = new List<string>();
        var sut = CreateSut(
            Credentials(models),
            request =>
            {
                requestedModels.Add(
                    request.RequestUri!.Segments[^1].Replace(":generateContent", "")
                );
                return JsonSuccess("The Tower");
            }
        );

        // Act
        for (var i = 0; i < 100; i++)
        {
            await sut.GenerateContentAsync(Prompt, CancellationToken.None);
        }

        // Assert
        requestedModels.Distinct().Should().Contain(models);
    }
}