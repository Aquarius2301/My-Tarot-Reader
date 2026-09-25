using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="AiTarotReadingService"/>. AI calls run through a mocked
/// <see cref="IGeminiClient"/>; persistence runs against a real <see cref="AppDbContext"/>
/// with the EF InMemory provider.
/// </summary>
public class AiTarotReadingServiceTests
{
    private const string ValidCard = "maj-00";

    #region Helpers

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static (
        AiTarotReadingService Service,
        AppDbContext Db,
        Mock<IGeminiClient> Gemini,
        Mock<IWalletService> Wallet
    ) CreateSut(int whiteBalance = 100)
    {
        var db = CreateInMemoryContext();
        var gemini = new Mock<IGeminiClient>();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(w => w.GetBalanceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetWalletBalanceResult(whiteBalance, 0));
        wallet
            .Setup(w =>
                w.DeductCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DeductCoinRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Guid _, DeductCoinRequest request, CancellationToken _) =>
                    new DeductCoinResult(request.Amount)
            );
        var service = new AiTarotReadingService(
            db,
            gemini.Object,
            wallet.Object,
            Options.Create(new AiTarotSetting()),
            new CreateAiTarotReadingRequestValidator()
        );
        return (service, db, gemini, wallet);
    }

    private static async Task SeedUserAsync(AppDbContext db, Guid userId)
    {
        var user = new User
        {
            Id = userId,
            FullName = $"User-{userId:N}",
            Email = $"user-{userId:N}@example.com",
            Picture = "http://pic",
            ProviderKey = $"provider-{userId:N}",
            Role = UserRole.Registered,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private static CreateAiTarotReadingRequest ValidRequest() =>
        new(
            CardCount.Three,
            QuestionType.Love,
            "vi",
            [
                new AiCardRequest(ValidCard, false),
                new AiCardRequest("maj-06", true),
                new AiCardRequest("min-cups-2", false),
            ]
        );

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

    private static async Task<AITarotReading> SeedReadingAsync(
        AppDbContext db,
        Guid userId,
        string cardsJson,
        bool deleted = false
    )
    {
        var reading = new AITarotReading
        {
            UserId = userId,
            Title = "Tình yêu sắp tới",
            CardCount = CardCount.Three,
            QuestionType = QuestionType.Love,
            Answer = BuildGeminiJson("Một câu trả lời dài đầy đủ."),
            AnswerSummary = "Một câu trả lời dài đầy đủ.",
            Cards = cardsJson,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AITarotReadings.Add(reading);
        await db.SaveChangesAsync();
        return reading;
    }

    #endregion

    #region CreateAiTarotReadingAsync

    /// <summary>
    /// A valid request with a successful Gemini response persists the reading
    /// (title, full answer, answer summary, cards) for the correct user.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_ValidRequest_SavesReading()
    {
        var (service, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Năng lượng chung của trải bài.";
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson(overview));
        var request = ValidRequest();

        var result = await service.CreateAiTarotReadingAsync(request, userId);

        var entity = Assert.Single(db.AITarotReadings);
        entity.Id.Should().NotBeEmpty();
        result.Id.Should().Be(entity.Id);
        entity.UserId.Should().Be(userId);
        entity.Title.Should().Be("Tình yêu sắp tới");
        entity.CardCount.Should().Be(CardCount.Three);
        entity.QuestionType.Should().Be(QuestionType.Love);
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

    /// <summary>
    /// A long overview is truncated to the summary limit for the list excerpt.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_LongOverview_TruncatesAnswerSummary()
    {
        var (service, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var longOverview = new string('a', 500);
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson(longOverview));

        await service.CreateAiTarotReadingAsync(ValidRequest(), userId);

        var entity = Assert.Single(db.AITarotReadings);
        entity.AnswerSummary.Should().HaveLength(300);
        entity.AnswerSummary.Should().Be(longOverview[..300]);
    }

    /// <summary>
    /// When Gemini returns a card name (e.g. "the_fool") instead of a canonical code, the stored
    /// answer's cardCode is rewritten to the matching drawn card's canonical code.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_AnswerCardCodeByName_StoresCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "the_fool"));

        await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AITarotReadings);
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    /// <summary>
    /// A name-style cardCode that is not among the drawn cards still resolves through the whole
    /// deck mapping (e.g. "five_of_wands" -> "min-wands-5").
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_AnswerCardCodeNameInDeck_ResolvesToCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "five_of_wands"));

        await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AITarotReadings);
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("min-wands-5");
    }

    /// <summary>
    /// An unrecognizable cardCode falls back to the drawn card at the same index so the client
    /// never receives an invalid code.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_AnswerCardCodeGarbage_FallsBackToDrawnCard()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung.", cardCode: "xyz-123"));

        await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AITarotReadings);
        JsonDocument.Parse(entity.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    /// <summary>
    /// When the Gemini call fails, an InternalServerException bubbles up and nothing
    /// is persisted.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_GeminiFails_ThrowsInternalServerAndDoesNotSave()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(
                new InternalServerException(
                    AiTarotErrorCode.GenerationFailed,
                    innerException: new Exception("boom")
                )
            );

        var act = async () => await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InternalServerException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.GenerationFailed);
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A malformed (non-JSON) Gemini response fails with InternalServerException and
    /// nothing is persisted.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_InvalidGeminiJson_ThrowsInternalServerAndDoesNotSave()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync("this is not json");

        var act = async () => await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InternalServerException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.GenerationFailed);
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request whose card list length does not match the CardCount is rejected with
    /// BadRequestException before Gemini is ever called.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_CardCountMismatch_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with
        {
            CardCount = CardCount.Five,
        };

        var act = async () => await service.CreateAiTarotReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g =>
                g.GenerateContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request with a locale other than "en"/"vi" is rejected with BadRequestException.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_InvalidLocale_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Locale = "fr" };

        var act = async () => await service.CreateAiTarotReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.InvalidLocale);
        gemini.Verify(
            g =>
                g.GenerateContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request with an invalid card code is rejected with BadRequestException before
    /// Gemini is ever called.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_InvalidCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with
        {
            Cards =
            [
                new AiCardRequest("fake-card", false),
                new AiCardRequest("maj-06", true),
                new AiCardRequest("min-cups-2", false),
            ],
        };

        var act = async () => await service.CreateAiTarotReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.InvalidCard);
        gemini.Verify(
            g =>
                g.GenerateContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// After a successful Gemini response, the reading is persisted and the white coin
    /// cost for the spread (3 cards → 2 coins) is deducted exactly once as an AITarot order.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_ValidRequest_DeductsCoinCost()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung của trải bài."));

        await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        wallet.Verify(
            w =>
                w.DeductCoinAsync(
                    It.IsAny<Guid>(),
                    new DeductCoinRequest(2, OrderType.AITarot),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AITarotReadings.Should().ContainSingle();
        db.Orders.Should().BeEmpty(); // the real order is recorded by WalletService.DeductCoinAsync
    }

    /// <summary>
    /// A larger spread is charged a higher cost (5 cards → 3 coins). Both the balance check
    /// and the deduction use the per-card-count cost.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_FiveCards_DeductsThreeCoins()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Năng lượng chung của trải bài."));
        var request = ValidRequest() with
        {
            CardCount = CardCount.Five,
            Cards =
            [
                new AiCardRequest(ValidCard, false),
                new AiCardRequest("maj-06", true),
                new AiCardRequest("min-cups-2", false),
                new AiCardRequest("maj-21", false),
                new AiCardRequest("min-wands-3", true),
            ],
        };

        await service.CreateAiTarotReadingAsync(request, Guid.NewGuid());

        wallet.Verify(
            w =>
                w.DeductCoinAsync(
                    It.IsAny<Guid>(),
                    new DeductCoinRequest(3, OrderType.AITarot),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AITarotReadings.Should().ContainSingle();
    }

    /// <summary>
    /// When the white coin balance is below the reading cost, a BadRequestException with the
    /// insufficientCoins code is thrown before Gemini is called, and nothing is persisted.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_InsufficientBalance_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, wallet) = CreateSut(whiteBalance: 1);
        gemini
            .Setup(g =>
                g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(BuildGeminiJson("Không nên gọi."));

        var act = async () =>
            await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == WalletErrorCode.InsufficientCoins);
        gemini.Verify(
            g =>
                g.GenerateContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        wallet.Verify(
            w => w.DeductCoinAsync(It.IsAny<Guid>(), It.IsAny<DeductCoinRequest>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A user with no wallet fails the balance check with the walletNotFound code before
    /// Gemini is called.
    /// </summary>
    [Fact]
    public async Task CreateAiTarotReading_NoWallet_ThrowsNotFoundAndSkipsGemini()
    {
        var (service, db, gemini, wallet) = CreateSut();
        wallet
            .Setup(w => w.GetBalanceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(WalletErrorCode.WalletNotFound));

        var act = async () =>
            await service.CreateAiTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == WalletErrorCode.WalletNotFound);
        gemini.Verify(
            g =>
                g.GenerateContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AITarotReadings.Should().BeEmpty();
    }

    #endregion

    #region GetAiTarotReadingByIdAsync

    /// <summary>
    /// An existing reading for the user is returned with the full answer and cards.
    /// </summary>
    [Fact]
    public async Task GetAiTarotReading_Exists_ReturnsFullReading()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        var result = await service.GetAiTarotReadingByIdAsync(userId, reading.Id);

        result.Id.Should().Be(reading.Id);
        result.Title.Should().Be(reading.Title);
        result.CardCount.Should().Be(CardCount.Three);
        result.Type.Should().Be(QuestionType.Love);
        result.Answer.Should().NotBeNullOrEmpty();
        JsonDocument.Parse(result.Answer)
            .RootElement.GetProperty("overview")
            .GetString()
            .Should()
            .Contain("trả lời dài đầy đủ");
        result.Cards.Should().ContainSingle();
        result.Cards[0].CardCode.Should().Be(ValidCard);
        result.Cards[0].IsReversed.Should().BeFalse();
    }

    /// <summary>
    /// An already-stored answer whose cardCode is a card name is normalized to the matching
    /// drawn card's canonical code when returned.
    /// </summary>
    [Fact]
    public async Task GetAiTarotReading_StoredAnswerWithNameCode_ReturnsCanonicalCode()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = new AITarotReading
        {
            UserId = userId,
            Title = "Tình yêu sắp tới",
            CardCount = CardCount.Three,
            QuestionType = QuestionType.Love,
            Answer = BuildGeminiJson("Một câu trả lời dài đầy đủ.", cardCode: "the_fool"),
            AnswerSummary = "Một câu trả lời dài đầy đủ.",
            Cards = """[{"cardCode":"maj-00","isReversed":false}]""",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.AITarotReadings.Add(reading);
        await db.SaveChangesAsync();

        var result = await service.GetAiTarotReadingByIdAsync(userId, reading.Id);

        JsonDocument.Parse(result.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    /// <summary>
    /// A reading id that does not exist throws NotFoundException with the
    /// AiTarotErrorCode.ReadingNotFound code.
    /// </summary>
    [Fact]
    public async Task GetAiTarotReading_NotFound_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();
        var userId = Guid.NewGuid();

        var act = async () =>
            await service.GetAiTarotReadingByIdAsync(userId, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A reading that exists but belongs to a different user is treated as not found.
    /// </summary>
    [Fact]
    public async Task GetAiTarotReading_BelongsToOtherUser_ThrowsNotFound()
    {
        var (service, db, _, _) = CreateSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        var readingB = await SeedReadingAsync(
            db,
            userB,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        var act = async () => await service.GetAiTarotReadingByIdAsync(userA, readingB.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A soft-deleted reading is excluded by the global query filter and treated as not found.
    /// </summary>
    [Fact]
    public async Task GetAiTarotReading_SoftDeleted_ThrowsNotFound()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]""",
            deleted: true
        );

        var act = async () => await service.GetAiTarotReadingByIdAsync(userId, reading.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.ReadingNotFound);
    }

    #endregion

    #region GetAllAiTarotReadingsAsync

    /// <summary>
    /// The list result maps every reading into an item with only the short excerpt —
    /// the full Answer is never exposed on the list DTO.
    /// </summary>
    [Fact]
    public async Task GetAllAiTarotReadings_HasReadings_ReturnsItemsWithoutFullAnswer()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var newest = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );
        var oldest = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-06","isReversed":true}]"""
        );
        oldest.CreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        await db.SaveChangesAsync();

        var result = await service.GetAllAiTarotReadingsAsync(userId);

        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(newest.Id);
        result.Items[0].AnswerSummary.Should().Be("Một câu trả lời dài đầy đủ.");
        result.Items[0].Cards.Should().ContainSingle(x => x.CardCode == ValidCard);
        typeof(GetAllAiTarotReadingItem)
            .GetProperty("Answer")
            .Should()
            .BeNull("the list DTO must not carry the full answer");
    }

    /// <summary>
    /// A user with no readings gets back an empty (non-null) list.
    /// </summary>
    [Fact]
    public async Task GetAllAiTarotReadings_NoReadings_ReturnsEmptyList()
    {
        var (service, _, _, _) = CreateSut();

        var result = await service.GetAllAiTarotReadingsAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Only the requested user's readings are returned, never another user's.
    /// </summary>
    [Fact]
    public async Task GetAllAiTarotReadings_OnlyReturnsOwnReadings()
    {
        var (service, db, _, _) = CreateSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        await SeedReadingAsync(db, userA, """[{"cardCode":"maj-00","isReversed":false}]""");
        await SeedReadingAsync(db, userB, """[{"cardCode":"maj-21","isReversed":true}]""");

        var result = await service.GetAllAiTarotReadingsAsync(userA);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Cards.Single().CardCode.Should().Be(ValidCard);
    }

    /// <summary>
    /// Soft-deleted readings are excluded by the global query filter.
    /// </summary>
    [Fact]
    public async Task GetAllAiTarotReadings_ExcludesSoftDeletedRecords()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        await SeedReadingAsync(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-06","isReversed":true}]""",
            deleted: true
        );

        var result = await service.GetAllAiTarotReadingsAsync(userId);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Cards.Single().CardCode.Should().Be(ValidCard);
    }

    /// <summary>
    /// Readings are returned newest-first (ordered by CreatedAt descending).
    /// </summary>
    [Fact]
    public async Task GetAllAiTarotReadings_ReturnsOrderedByCreatedAtDescending()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var oldest = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );
        var newest = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-06","isReversed":true}]"""
        );
        oldest.CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);
        newest.CreatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var result = await service.GetAllAiTarotReadingsAsync(userId);

        result.Items.Select(x => x.Id).Should().Equal(newest.Id, oldest.Id);
    }

    #endregion

    #region DeleteAiTarotReadingAsync

    /// <summary>
    /// Deleting an existing reading soft-deletes it (DeletedAt set) and makes it
    /// disappear from further GetAllAiTarotReadingsAsync results.
    /// </summary>
    [Fact]
    public async Task DeleteAiTarotReading_ExistingReading_SoftDeletes()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        await service.DeleteAiTarotReadingAsync(userId, reading.Id);

        db.AITarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        var afterDelete = await service.GetAllAiTarotReadingsAsync(userId);
        afterDelete.Items.Should().BeEmpty();
    }

    /// <summary>
    /// A reading id that does not exist throws NotFoundException with the
    /// AiTarotErrorCode.ReadingNotFound code.
    /// </summary>
    [Fact]
    public async Task DeleteAiTarotReading_ReadingNotFound_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();
        var userId = Guid.NewGuid();

        var act = async () => await service.DeleteAiTarotReadingAsync(userId, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A reading that exists but belongs to a different user is treated as not found
    /// (NotFoundException) and the other user's reading is left untouched.
    /// </summary>
    [Fact]
    public async Task DeleteAiTarotReading_BelongsToOtherUser_ThrowsNotFound()
    {
        var (service, db, _, _) = CreateSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        var readingB = await SeedReadingAsync(
            db,
            userB,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        var act = async () => await service.DeleteAiTarotReadingAsync(userA, readingB.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiTarotErrorCode.ReadingNotFound);
        db.AITarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }

    #endregion
}