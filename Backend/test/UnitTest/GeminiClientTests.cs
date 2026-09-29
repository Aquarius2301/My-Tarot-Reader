using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Settings;
using MyTarotReader.Infrastructure.Common;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="GeminiClient"/> retry and failover behavior, exercised through a
/// stubbed <see cref="HttpMessageHandler"/> so no real network call and no real delay is made.
/// </summary>
public class GeminiClientTests
{
    private const string Prompt = "Interpret this spread";

    #region Helpers

    private sealed record Call(string ApiKey, string Model, string? ThinkingLevel);

    private sealed record Harness(TestableGeminiClient Sut, List<Call> Calls);

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

    private static HttpResponseMessage Error(HttpStatusCode status, TimeSpan? retryAfter = null)
    {
        var response = new HttpResponseMessage { StatusCode = status };
        if (retryAfter is { } value)
        {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(value);
        }

        return response;
    }

    private static List<GeminiModelSetting> Models(string[] models, string level = "low") =>
        models.Select(model => new GeminiModelSetting { Model = model, ThinkingLevel = level }).ToList();

    private static Harness CreateSut(
        List<string> apis,
        List<GeminiModelSetting> models,
        Func<Call, HttpResponseMessage> handler,
        int maxRetries = 2,
        int retryDelayMilliseconds = 1000
    )
    {
        var calls = new List<Call>();
        var httpClient = new HttpClient(
            new StubHttpMessageHandler(async request =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                using var document = JsonDocument.Parse(body);
                var generationConfig = document.RootElement.GetProperty("generationConfig");
                var call = new Call(
                    request.Headers.GetValues("x-goog-api-key").Single(),
                    request.RequestUri!.Segments[^1].Replace(":generateContent", ""),
                    generationConfig.TryGetProperty("thinkingConfig", out var thinking)
                        ? thinking.GetProperty("thinkingLevel").GetString()
                        : null
                );

                calls.Add(call);
                return handler(call);
            })
        );

        var setting = Options.Create(
            new GeminiSetting
            {
                MaxRetries = maxRetries,
                RetryDelayMilliseconds = retryDelayMilliseconds,
                MaxTotalWaitSeconds = 0,
                Apis = apis,
                Models = models,
            }
        );

        return new Harness(new TestableGeminiClient(httpClient, setting), calls);
    }

    private sealed class TestableGeminiClient(HttpClient httpClient, IOptions<GeminiSetting> setting)
        : GeminiClient(httpClient, setting, NullLogger<GeminiClient>.Instance)
    {
        public List<TimeSpan> Delays { get; } = [];

        protected override Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            Delays.Add(delay);
            return Task.CompletedTask;
        }
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => handler(request);
    }

    #endregion

    [Fact]
    public async Task GenerateContentAsync_FirstPairSucceeds_ReturnsText()
    {
        // Arrange
        var harness = CreateSut(["key-0"], Models(["gemini-3.8-flash"]), _ => JsonSuccess("The Lovers"));

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Lovers");
        harness
            .Calls.Should()
            .BeEquivalentTo([new Call("key-0", "gemini-3.8-flash", "low")]);
    }

    [Fact]
    public async Task GenerateContentAsync_ServiceUnavailable_RetriesAfterBaseDelay()
    {
        // Arrange
        var attempts = 0;
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash"]),
            _ => ++attempts == 1 ? Error(HttpStatusCode.ServiceUnavailable) : JsonSuccess("The Sun")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Sun");
        harness.Calls.Should().HaveCount(2);
        harness.Sut.Delays.Should().ContainSingle().Which.Should().Be(TimeSpan.FromMilliseconds(1000));
    }

    [Fact]
    public async Task GenerateContentAsync_ServiceUnavailable_AppliesExponentialBackoffThenFailsOverToNextModel()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            call => call.Model == "gemini-3.8-flash"
                ? Error(HttpStatusCode.ServiceUnavailable)
                : JsonSuccess("The Star")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Star");
        harness
            .Calls.Select(call => call.Model)
            .Should()
            .Equal("gemini-3.8-flash", "gemini-3.8-flash", "gemini-3.8-flash", "gemini-3.5-flash");
        harness.Sut.Delays.Should().HaveCount(2);
        harness.Sut.Delays[0].Should().Be(TimeSpan.FromMilliseconds(1000));
        harness.Sut.Delays[1].Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(2000));
        harness.Sut.Delays[1].Should().BeLessThan(TimeSpan.FromMilliseconds(3000));
    }

    [Fact]
    public async Task GenerateContentAsync_TooManyRequests_RetriesThenSwitchesToNextApi()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0", "key-1"],
            Models(["gemini-3.8-flash"]),
            call => call.ApiKey == "key-0" ? Error(HttpStatusCode.TooManyRequests) : JsonSuccess("The Moon")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Moon");
        harness
            .Calls.Select(call => call.ApiKey)
            .Should()
            .Equal("key-0", "key-0", "key-0", "key-1");
        harness.Sut.Delays.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateContentAsync_TooManyRequestsWithoutRetries_SwitchesToNextApi()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0", "key-1"],
            Models(["gemini-3.8-flash"]),
            call => call.ApiKey == "key-0" ? Error(HttpStatusCode.TooManyRequests) : JsonSuccess("The Moon"),
            maxRetries: 0
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Moon");
        harness.Calls.Select(call => call.ApiKey).Should().Equal("key-0", "key-1");
        harness.Sut.Delays.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateContentAsync_TooManyRequestsWithRetryAfter_WaitsForTheServerValue()
    {
        // Arrange
        var attempts = 0;
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash"]),
            _ => ++attempts == 1
                ? Error(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(2))
                : JsonSuccess("The Tower")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Tower");
        harness.Sut.Delays.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GenerateContentAsync_ApiNotUsable_SwitchesToNextApiWithoutRetrying(
        HttpStatusCode status
    )
    {
        // Arrange
        var harness = CreateSut(
            ["key-0", "key-1"],
            Models(["gemini-3.8-flash"]),
            call => call.ApiKey == "key-0" ? Error(status) : JsonSuccess("The Hermit")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Hermit");
        harness.Calls.Select(call => call.ApiKey).Should().Equal("key-0", "key-1");
        harness.Sut.Delays.Should().BeEmpty();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task GenerateContentAsync_ModelNotUsable_SwitchesToNextModelWithoutRetrying(
        HttpStatusCode status
    )
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            call => call.Model == "gemini-3.8-flash" ? Error(status) : JsonSuccess("The World")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The World");
        harness.Calls.Select(call => call.Model).Should().Equal("gemini-3.8-flash", "gemini-3.5-flash");
        harness.Sut.Delays.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateContentAsync_EmptyResponse_FailsOverWithoutRetrying()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            call => call.Model == "gemini-3.8-flash" ? JsonEmpty() : JsonSuccess("The Fool")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Fool");
        harness.Calls.Select(call => call.Model).Should().Equal("gemini-3.8-flash", "gemini-3.5-flash");
        harness.Sut.Delays.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateContentAsync_TransportError_RetriesThenFailsOverToNextModel()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            call =>
                call.Model == "gemini-3.8-flash"
                    ? throw new HttpRequestException("connection reset")
                    : JsonSuccess("The Emperor")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Emperor");
        harness.Calls.Should().HaveCount(4);
        harness.Sut.Delays.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateContentAsync_AllPairsFail_TriesEveryPairOnceWithItsRetries()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0", "key-1"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            _ => Error(HttpStatusCode.ServiceUnavailable)
        );

        // Act
        var act = () => harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
        harness.Calls.Should().HaveCount(4 * 3);
        harness.Sut.Delays.Should().HaveCount(4 * 2);
        harness.Calls.Select(call => (call.ApiKey, call.Model)).Distinct().Should().HaveCount(4);
    }

    [Fact]
    public async Task GenerateContentAsync_MaxRetriesZero_CallsEveryPairOnce()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            Models(["gemini-3.8-flash", "gemini-3.5-flash"]),
            _ => Error(HttpStatusCode.ServiceUnavailable),
            maxRetries: 0
        );

        // Act
        var act = () => harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
        harness.Calls.Select(call => call.Model).Should().Equal("gemini-3.8-flash", "gemini-3.5-flash");
        harness.Sut.Delays.Should().BeEmpty();
    }

    [Theory]
    [InlineData("gemini-2.5-flash", "low")]
    [InlineData("gemini-3.8-flash", "minimal")]
    [InlineData("gemini-3.1-pro-preview", "minimal")]
    [InlineData("gemini-3.1-flash-lite-image", "low")]
    public async Task GenerateContentAsync_UnsupportedThinkingLevel_OmitsThinkingConfig(
        string model,
        string level
    )
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            [new GeminiModelSetting { Model = model, ThinkingLevel = level }],
            _ => JsonSuccess("The Star")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Star");
        harness.Calls.Should().ContainSingle().Which.ThinkingLevel.Should().BeNull();
    }

    [Theory]
    [InlineData("gemini-3.8-flash", "low", "low")]
    [InlineData("gemini-3.5-flash", "MINIMAL", "minimal")]
    [InlineData("gemini-3.1-flash-lite-image", "high", "high")]
    public async Task GenerateContentAsync_SupportedThinkingLevel_SendsNormalizedLevel(
        string model,
        string level,
        string expected
    )
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            [new GeminiModelSetting { Model = model, ThinkingLevel = level }],
            _ => JsonSuccess("The Star")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Star");
        harness.Calls.Should().ContainSingle().Which.ThinkingLevel.Should().Be(expected);
    }

    [Fact]
    public async Task GenerateContentAsync_WithoutThinkingLevel_OmitsThinkingConfig()
    {
        // Arrange
        var harness = CreateSut(
            ["key-0"],
            [new GeminiModelSetting { Model = "gemini-3.8-flash" }],
            _ => JsonSuccess("The Star")
        );

        // Act
        var result = await harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        result.Should().Be("The Star");
        harness.Calls.Should().ContainSingle().Which.ThinkingLevel.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateContentAsync_BlankApiKeys_AreIgnored(string apiKey)
    {
        // Arrange
        var harness = CreateSut(
            [apiKey],
            Models(["gemini-3.8-flash"]),
            _ => JsonSuccess("unreachable")
        );

        // Act
        var act = () => harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task GenerateContentAsync_NoModelsConfigured_ThrowsInternalServerException()
    {
        // Arrange
        var harness = CreateSut(["key-0"], [], _ => JsonSuccess("unreachable"));

        // Act
        var act = () => harness.Sut.GenerateContentAsync(Prompt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }
}
