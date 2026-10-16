using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

/// <summary>
/// Unit tests for <see cref="CreateAiTarotReadingCommandHandler"/>. Gemini and wallet calls
/// run through mocks; persistence runs against a real <see cref="AppDbContext"/> with the
/// EF InMemory provider.
/// </summary>
public class CreateAiTarotReadingCommandHandlerTests
{
    private const string ValidCard = "maj-00";

    #region Helpers

    private static (
        CreateAiTarotReadingCommandHandler Sut,
        AppDbContext Db,
        Mock<IGeminiClientService> Gemini,
        Mock<IWalletService> Wallet
    ) CreateSut(int whiteBalance = 100)
    {
        var db = TestDbContextFactory.Create();
        SeedSettings(db);
        var gemini = new Mock<IGeminiClientService>();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(w => w.GetCoinsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCoins(0, whiteBalance));
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

        var sut = new CreateAiTarotReadingCommandHandler(
            db,
            wallet.Object,
            gemini.Object
        );

        return (sut, db, gemini, wallet);
    }

    private static void SeedSettings(AppDbContext db)
    {
        db.AITarotReadingCounts.AddRange(
            new AITarotReadingCount
            {
                CardCount = 3,
                WhiteCoins = 2,
                PositionsEn = ["Core energy", "Challenges/obstacles", "Outcome/advice"],
                PositionsVi = ["Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên"],
                MinWords = 300,
                MaxWords = 400,
                IsEnabled = true,
            },
            new AITarotReadingCount
            {
                CardCount = 5,
                WhiteCoins = 3,
                PositionsEn =
                [
                    "Core energy",
                    "Challenges/obstacles",
                    "Your strength",
                    "Future",
                    "Outcome/advice",
                ],
                PositionsVi =
                [
                    "Năng lượng cốt lõi",
                    "Thử thách/trở ngại",
                    "Điểm mạnh của bạn",
                    "Tương lai",
                    "Kết quả/lời khuyên",
                ],
                MinWords = 450,
                MaxWords = 600,
                IsEnabled = true,
            }
        );
        db.AITarotReadingTypes.Add(
            new AiTarotReadingType
            {
                Code = "love",
                NameEn = "Love",
                NameVi = "Tình yêu",
                IsEnabled = true,
            }
        );
        db.SaveChanges();
    }

    private static List<AiReadingCard> ValidCards() =>
        [new(ValidCard, false), new("maj-06", true), new("min-cups-2", false)];

    private static CreateAiTarotReadingCommand ValidCommand(Guid userId) =>
        new(3, "love", "vi", ValidCards(), userId);

    private static string BuildGeminiJson(string overview, string cardCode = ValidCard) =>
        JsonSerializer.Serialize(
            new
            {
                title = "Tình yêu sắp tới",
                overview,
                cards = new[]
                {
                    new
                    {
                        cardCode,
                        position = "Quá khứ",
                        interpretation = "Bạn đang bắt đầu một giai đoạn mới.",
                    },
                },
                overallAdvice = "Hãy lắng nghe trực giác của bạn.",
            }
        );

    #endregion

    [Fact]
    public async Task Handle_ValidRequest_SavesReading()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Năng lượng chung của trải bài.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(overview));

        // Act
        var result = await sut.Handle(ValidCommand(userId), CancellationToken.None);

        // Assert
        var entity = db.AITarotReadings.Should().ContainSingle().Subject;
        entity.Id.Should().NotBeEmpty();
        result.Id.Should().Be(entity.Id);
        entity.UserId.Should().Be(userId);
        entity.Title.Should().Be("Tình yêu sắp tới");
        entity.CardCount.Should().Be(3);
        entity.QuestionType.Should().Be("love");
        entity.Answer.Should().NotBeNullOrEmpty();

        var parsedAnswer = JsonDocument.Parse(entity.Answer);
        parsedAnswer.RootElement.GetProperty("overview").GetString().Should().Be(overview);
        parsedAnswer.RootElement.GetProperty("title").GetString().Should().Be("Tình yêu sắp tới");

        entity.AnswerSummary.Should().Be(overview);

        var cards = JsonSerializer.Deserialize<List<AiReadingCard>>(
            entity.Cards,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        cards.Should().HaveCount(3);
        cards![0].CardCode.Should().Be(ValidCard);
        cards[0].IsReversed.Should().BeFalse();
        cards[1].CardCode.Should().Be("maj-06");
        cards[1].IsReversed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LongOverview_TruncatesAnswerSummary()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var longOverview = new string('a', 500);
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(longOverview));

        // Act
        await sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var entity = db.AITarotReadings.Should().ContainSingle().Subject;
        entity.AnswerSummary.Should().HaveLength(300);
        entity.AnswerSummary.Should().Be(longOverview[..300]);
    }

    [Fact]
    public async Task Handle_AnswerCardCodeByName_StoresCanonicalCode()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "the_fool"));

        // Act
        await sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var entity = db.AITarotReadings.Should().ContainSingle().Subject;
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    [Fact]
    public async Task Handle_AnswerCardCodeNameInDeck_ResolvesToCanonicalCode()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "five_of_wands"));

        // Act
        await sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var entity = db.AITarotReadings.Should().ContainSingle().Subject;
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("min-wands-5");
    }

    [Fact]
    public async Task Handle_AnswerCardCodeGarbage_FallsBackToDrawnCard()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "xyz-123"));

        // Act
        await sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var entity = db.AITarotReadings.Should().ContainSingle().Subject;
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
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
                    AiTarotErrorCode.GenerationFailed,
                    innerException: new Exception("boom")
                )
            );

        // Act
        var act = () => sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InternalServerException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.GenerationFailed);
        db.AITarotReadings.Should().BeEmpty();
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
        var act = () => sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InternalServerException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.GenerationFailed);
        db.AITarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UnsupportedCardCount_ThrowsBadRequestAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var command = ValidCommand(Guid.NewGuid()) with { CardCount = 99 };

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UnsupportedQuestionType_ThrowsBadRequestAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, _) = CreateSut();
        var command = ValidCommand(Guid.NewGuid()) with { Type = "unknown" };

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.InvalidQuestionType);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_DeductsCoinCost()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung của trải bài."));

        // Act
        await sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    0,
                    2,
                    OrderType.AITarot,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AITarotReadings.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_FiveCards_DeductsThreeCoins()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung của trải bài."));
        var command = ValidCommand(Guid.NewGuid()) with
        {
            CardCount = 5,
            Cards =
            [
                new(ValidCard, false),
                new("maj-06", true),
                new("min-cups-2", false),
                new("maj-21", false),
                new("min-wands-3", true),
            ],
        };

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        wallet.Verify(
            w =>
                w.DeductCoinsAsync(
                    It.IsAny<Guid>(),
                    0,
                    3,
                    OrderType.AITarot,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AITarotReadings.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_InsufficientBalance_ThrowsBadRequestAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut(whiteBalance: 1);
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Không nên gọi."));

        // Act
        var act = () => sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.InsufficientCoins);
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
        db.AITarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoWallet_ThrowsNotFoundAndSkipsGemini()
    {
        // Arrange
        var (sut, db, gemini, wallet) = CreateSut();
        wallet
            .Setup(w => w.GetCoinsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(WalletErrorCode.WalletNotFound));

        // Act
        var act = () => sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.WalletNotFound);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }
}
