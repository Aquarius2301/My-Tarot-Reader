using System.Text.Json;
using FluentAssertions;
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

public class CreateTwelveMonthsReadingCommandHandlerTests
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
        CreateTwelveMonthsReadingCommandHandler Sut,
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

        var sut = new CreateTwelveMonthsReadingCommandHandler(db, gemini.Object, wallet.Object);
        return (sut, db, gemini, wallet);
    }

    private static List<AiDeepCardRequest> Cards(int count, int reversalStep) =>
        TwelveCardCodes
            .Take(count)
            .Select((code, index) => new AiDeepCardRequest(code, index % reversalStep == 0))
            .ToList();

    private static CreateTwelveMonthsReadingCommand ValidCommand(string locale = "vi") =>
        new(locale, Cards(12, reversalStep: 3), Guid.NewGuid());

    private static string BuildMonthsGeminiJson(string overview, DateTimeOffset createdAt)
    {
        var positions = DeepTarotConstant.GetPositions(DeepTarotTopic.TwelveMonths);

        return JsonSerializer.Serialize(
            new
            {
                title = "Vận mệnh 12 tháng tới",
                overview,
                sections = positions
                    .Select(
                        (position, index) =>
                            new
                            {
                                key = position.Key,
                                title =
                                    $"{DeepTarotConstant.GetMonthLabel(position, createdAt)} - Tháng {position.Number}",
                                cardCode = TwelveCardCodes[index],
                                interpretation = $"Diễn giải cho tháng {position.Number}.",
                            }
                    )
                    .ToArray(),
                overallAdvice = "Hãy tin vào trực giác của bạn.",
            }
        );
    }

    private static JsonElement ParseAnswer(string answer) =>
        JsonDocument.Parse(answer).RootElement;

    [Fact]
    public async Task Handle_ValidRequest_SavesReading()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Một năm tới với 12 tháng liên tục.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson(overview, DateTimeOffset.UtcNow));

        // Act
        var result = await sut.Handle(
            new CreateTwelveMonthsReadingCommand("vi", Cards(12, 3), userId),
            CancellationToken.None
        );

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        entity.Topic.Should().Be(DeepTarotTopic.TwelveMonths);
        entity.UserId.Should().Be(userId);
        entity.Title.Should().Be("Vận mệnh 12 tháng tới");
        entity.AnswerSummary.Should().Be(overview);
        result.Should().Be(entity.Id);

        var answer = ParseAnswer(entity.Answer);
        answer.GetProperty("overview").GetString().Should().Be(overview);
        var sections = answer.GetProperty("sections").EnumerateArray().ToList();
        sections.Should().HaveCount(12);
        sections
            .Select(s => s.GetProperty("key").GetString())
            .Should()
            .Equal(Enumerable.Range(1, 12).Select(i => $"month-{i}"));
    }

    [Fact]
    public async Task Handle_PromptContainsMonthKeysAndNextMonthLabels()
    {
        // Arrange
        var (sut, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        for (var i = 1; i <= 12; i++)
        {
            rawPrompt.Should().Contain($"month-{i}");
        }

        var expectedLabels = DeepTarotConstant
            .TwelveMonthsPositions.Select(p => DeepTarotConstant.GetMonthLabel(p, DateTimeOffset.UtcNow))
            .ToList();
        expectedLabels.Should().OnlyHaveUniqueItems();
        expectedLabels
            .Select(label => label.ToString())
            .Should()
            .AllSatisfy(label => rawPrompt.Should().Contain(label));
        rawPrompt.Should().Contain("MM/yyyy");
    }

    [Fact]
    public async Task Handle_PromptDoesNotMentionHouses()
    {
        // Arrange
        var (sut, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        rawPrompt.Should().NotContain("house-");
        rawPrompt.Should().NotContain("astrological");
        rawPrompt.Should().Contain("MM/yyyy");
    }

    [Fact]
    public async Task Handle_DeductsThreeRedCoins()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        DeepTarotConstant.GetCost(DeepTarotTopic.TwelveMonths).Should().Be(3);
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
    public async Task Handle_ExactRedCoinBalance_Succeeds()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut(
            redCoin: DeepTarotConstant.GetCost(DeepTarotTopic.TwelveMonths)
        );
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        db.AIDeepTarotReadings.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_InsufficientRedCoin_ThrowsBadRequestAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut(redCoin: 2);

        // Act
        var act = () => sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.InsufficientRedCoins);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SectionKeyUnknown_FallsBackToMonthKey()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        title = "Vận mệnh 12 tháng tới",
                        overview = "Tổng quan.",
                        sections = Enumerable
                            .Range(1, 12)
                            .Select(index => new
                            {
                                key = "garbage-key",
                                title = $"Tháng {index}",
                                cardCode = TwelveCardCodes[index - 1],
                                interpretation = $"Diễn giải tháng {index}.",
                            })
                            .ToArray(),
                        overallAdvice = "Chúc bạn nhiều may mắn.",
                    }
                )
            );

        // Act
        await sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        var entity = db.AIDeepTarotReadings.Should().ContainSingle().Subject;
        var sections = ParseAnswer(entity.Answer).GetProperty("sections").EnumerateArray().ToList();
        sections
            .Select(s => s.GetProperty("key").GetString())
            .Should()
            .Equal(Enumerable.Range(1, 12).Select(i => $"month-{i}"));
    }
}
