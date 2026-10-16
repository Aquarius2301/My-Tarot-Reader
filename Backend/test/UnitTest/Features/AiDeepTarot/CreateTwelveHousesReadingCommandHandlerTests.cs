using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Features.AiDeepTarot;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class CreateTwelveHousesReadingCommandHandlerTests
{
    private static readonly string[] TwelveCardCodes =
    [
        "maj-00",
        "maj-01",
        "maj-02",
        "maj-03",
        "maj-04",
        "maj-05",
        "maj-06",
        "maj-07",
        "maj-08",
        "maj-09",
        "maj-10",
        "maj-11",
    ];

    private static (
        CreateTwelveHousesReadingCommandHandler Sut,
        AppDbContext Db,
        Mock<IGeminiClientService> Gemini,
        Mock<IWalletService> Wallet
    ) CreateSut(int redCoin = 10)
    {
        var db = TestDbContextFactory.Create();
        var gemini = new Mock<IGeminiClientService>();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(w => w.GetCoinsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCoins(redCoin, 0));
        wallet
            .Setup(w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<OrderType>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var sut = new CreateTwelveHousesReadingCommandHandler(db, gemini.Object, wallet.Object);
        return (sut, db, gemini, wallet);
    }

    private static List<AiDeepCardRequest> Cards(int count, int reversalStep) =>
        TwelveCardCodes
            .Take(count)
            .Select((code, index) => new AiDeepCardRequest(code, index % reversalStep == 0))
            .ToList();

    private static CreateTwelveHousesReadingCommand ValidCommand(string locale = "vi") =>
        new(locale, Cards(12, reversalStep: 3), Guid.NewGuid());

    private static string BuildGeminiJson(
        string overview,
        IReadOnlyDictionary<int, string>? sectionCardCodes = null
    ) =>
        JsonSerializer.Serialize(
            new
            {
                title = "Vận mệnh qua 12 nhà",
                overview,
                sections = Enumerable
                    .Range(1, 12)
                    .Select(index => new
                    {
                        key = $"house-{index}",
                        title = $"Nhà {index}",
                        cardCode =
                            sectionCardCodes is not null
                            && sectionCardCodes.TryGetValue(index, out var code)
                                ? code
                                : TwelveCardCodes[index - 1],
                        interpretation = $"Diễn giải cho nhà {index}.",
                    })
                    .ToArray(),
                overallAdvice = "Hãy tin vào trực giác của bạn.",
            }
        );

    private static JsonElement ParseAnswer(string answer) =>
        JsonDocument.Parse(answer).RootElement;

    [Fact]
    public async Task Handle_ValidRequest_SavesReading()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Toàn bộ cung chiêm tinh đi từ bản thân đến tiềm thức.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(overview));
        var command = new CreateTwelveHousesReadingCommand("vi", Cards(12, 3), userId);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        entity.Id.Should().NotBeEmpty();
        result.Should().Be(entity.Id);
        entity.UserId.Should().Be(userId);
        entity.Topic.Should().Be(DeepTarotTopic.TwelveHouses);
        entity.Title.Should().Be("Vận mệnh qua 12 nhà");
        entity.AnswerSummary.Should().Be(overview);

        var answer = ParseAnswer(entity.Answer);
        answer.GetProperty("overview").GetString().Should().Be(overview);
        answer.GetProperty("title").GetString().Should().Be("Vận mệnh qua 12 nhà");
        answer.GetProperty("sections").GetArrayLength().Should().Be(12);

        var cards = JsonSerializer.Deserialize<List<AiDeepReadingCard>>(
            entity.Cards,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        cards.Should().HaveCount(12);
        cards![0].CardCode.Should().Be("maj-00");
        cards[0].IsReversed.Should().BeTrue();
        cards[1].IsReversed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidRequest_PromptContainsAllHouseKeysAndCards()
    {
        // Arrange
        var (sut, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        for (var i = 1; i <= 12; i++)
        {
            rawPrompt.Should().Contain($"house-{i}");
        }

        TwelveCardCodes
            .Select(TarotConstant.GetCardName)
            .Should()
            .AllSatisfy(name => rawPrompt.Should().Contain(name));
        rawPrompt
            .Should()
            .Contain("Reversed")
            .And.Contain("Upright")
            .And.Contain("Vietnamese");
    }

    [Theory]
    [InlineData("vi", "Vietnamese")]
    [InlineData("en", "English")]
    public async Task Handle_Locale_SetsPromptLanguage(string locale, string expected)
    {
        // Arrange
        var (sut, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Overview."));

        // Act
        await sut.Handle(ValidCommand(locale), CancellationToken.None);

        // Assert
        rawPrompt.Should().Contain(expected);
    }

    [Fact]
    public async Task Handle_PromptDoesNotMentionMonths()
    {
        // Arrange
        var (sut, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        rawPrompt.Should().NotContain("month-");
        rawPrompt.Should().NotContain("MM/yyyy");
        rawPrompt.Should().Contain("astrological");
    }

    [Fact]
    public async Task Handle_LongOverview_TruncatesAnswerSummary()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var longOverview = new string('a', 800);
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(longOverview));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        entity.AnswerSummary.Should().HaveLength(500);
        entity.AnswerSummary.Should().Be(longOverview[..500]);
    }

    [Fact]
    public async Task Handle_SectionCardCodeByName_StoresCanonicalCode()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "the_fool" })
            );

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    [Fact]
    public async Task Handle_SectionCardCodeNameInDeck_ResolvesToCanonicalCode()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "five_of_wands" })
            );

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("min-wands-5");
    }

    [Fact]
    public async Task Handle_SectionCardCodeGarbage_FallsBackToDrawnCard()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "xyz-123" })
            );

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    [Fact]
    public async Task Handle_SectionKeyUnknown_FallsBackToPositionKey()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var rawAnswer = JsonSerializer.Serialize(
            new
            {
                title = "Vận mệnh qua 12 nhà",
                overview = "Tổng quan.",
                sections = new[]
                {
                    new
                    {
                        key = "house-what",
                        title = "Nhà 1",
                        cardCode = "maj-00",
                        interpretation = "Diễn giải nhà 1.",
                    },
                },
                overallAdvice = "Lời khuyên.",
            }
        );
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rawAnswer);

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("key")
            .GetString()
            .Should()
            .Be("house-1");
    }

    [Fact]
    public async Task Handle_GeminiFails_ThrowsInternalServerAndDoesNotSave()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InternalServerException(
                    AiDeepTarotErrorCode.GenerationFailed,
                    innerException: new Exception("boom")
                )
            );

        // Act
        var act = () => sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InternalServerException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.GenerationFailed);
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidGeminiJson_ThrowsInternalServerAndDoesNotSave()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("this is not json");

        // Act
        var act = () => sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InternalServerException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.GenerationFailed);
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_DeductsThreeRedCoins()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    3,
                    0,
                    OrderType.AIDeepTarot,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AIDeepTarotReadings.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ValidRequest_ChargesTheTopicCost()
    {
        // Arrange
        var (sut, _, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));
        var cost = DeepTarotConstant.GetCost(DeepTarotTopic.TwelveHouses);

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    cost,
                    0,
                    OrderType.AIDeepTarot,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_InsufficientRedCoin_ThrowsBadRequestAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut(redCoin: 2);

        // Act
        var act = () => sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.InsufficientRedCoins);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<OrderType>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExactRedCoinBalance_Succeeds()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut(
            redCoin: DeepTarotConstant.GetCost(DeepTarotTopic.TwelveHouses)
        );
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        db.AIDeepTarotReadings.Should().ContainSingle();
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    OrderType.AIDeepTarot,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_GeminiFails_DeductsNoRedCoin()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("gemini down"));

        // Act
        var act = () => sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<OrderType>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }
}
