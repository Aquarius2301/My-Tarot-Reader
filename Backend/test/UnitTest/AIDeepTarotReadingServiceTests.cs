using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="AIDeepTarotReadingService"/>. AI calls run through a mocked
/// <see cref="IGeminiClient"/>; persistence runs against a real <see cref="AppDbContext"/>
/// with the EF InMemory provider.
/// </summary>
public class AIDeepTarotReadingServiceTests
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
        AIDeepTarotReadingService Service,
        AppDbContext Db,
        Mock<IGeminiClient> Gemini,
        Mock<IWalletService> Wallet
    ) CreateSut()
    {
        var db = CreateInMemoryContext();
        var gemini = new Mock<IGeminiClient>();
        var wallet = new Mock<IWalletService>();
        SetupRedCoinBalance(wallet, redCoin: 10);
        wallet
            .Setup(w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DeductRedCoinRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Guid _, DeductRedCoinRequest request, CancellationToken _) =>
                    new DeductRedCoinResult(10 - request.Amount)
            );
        var service = new AIDeepTarotReadingService(
            db,
            gemini.Object,
            wallet.Object,
            new CreateTwelveHousesReadingRequestValidator(),
            new CreateTwelveMonthsReadingRequestValidator()
        );
        return (service, db, gemini, wallet);
    }

    /// <summary>
    /// Overrides the red coin balance reported by the mocked wallet service.
    /// </summary>
    private static void SetupRedCoinBalance(Mock<IWalletService> wallet, int redCoin) =>
        wallet
            .Setup(w =>
                w.GetBalanceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GetWalletBalanceResult(WhiteCoin: 0, RedCoin: redCoin));

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

    /// <summary>
    /// A valid 12-house request, using one of each of the first 12 major arcana.
    /// </summary>
    private static CreateTwelveHousesReadingRequest ValidRequest(string locale = "vi") =>
        new(locale, Cards(12, reversalStep: 3));

    /// <summary>
    /// A valid 12-months request, using the same cards as the houses spread.
    /// </summary>
    private static CreateTwelveMonthsReadingRequest ValidMonthsRequest(string locale = "vi") =>
        new(locale, Cards(12, reversalStep: 3));

    private static List<AiDeepCardRequest> Cards(int count, int reversalStep) =>
        TwelveCardCodes
            .Take(count)
            .Select((code, index) => new AiDeepCardRequest(code, index % reversalStep == 0))
            .ToList();

    /// <summary>
    /// Builds a Gemini payload with one section per house key, optionally overriding
    /// the cardCode reported for a given section.
    /// </summary>
    private static string BuildGeminiJson(
        string overview,
        IReadOnlyDictionary<int, string>? sectionCardCodes = null
    )
    {
        return JsonSerializer.Serialize(
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
                        cardCode = sectionCardCodes is not null
                            && sectionCardCodes.TryGetValue(index, out var code)
                            ? code
                            : TwelveCardCodes[index - 1],
                        interpretation = $"Diễn giải cho nhà {index}.",
                    })
                    .ToArray(),
                overallAdvice = "Hãy tin vào trực giác của bạn.",
            }
        );
    }

    private static async Task<AIDeepTarotReading> SeedReadingAsync(
        AppDbContext db,
        Guid userId,
        string cardsJson,
        bool deleted = false
    )
    {
        var reading = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = DeepTarotTopic.TwelveHouses,
            Title = "Vận mệnh qua 12 nhà",
            Answer = BuildGeminiJson("Một tổng quan đầy đủ về cả 12 nhà."),
            AnswerSummary = "Một tổng quan đầy đủ về cả 12 nhà.",
            Cards = cardsJson,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AIDeepTarotReadings.Add(reading);
        await db.SaveChangesAsync();
        return reading;
    }

    private static JsonElement ParseAnswer(string answer) => JsonDocument.Parse(answer).RootElement;

    /// <summary>
    /// Builds a Gemini payload with one section per month key, titled with the resolved
    /// calendar month label.
    /// </summary>
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

    #endregion

    #region CreateTwelveHousesReadingAsync

    /// <summary>
    /// A valid 12-house request with a successful Gemini response persists the reading
    /// (topic, title, full answer, answer summary, 12 cards) for the correct user.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_ValidRequest_SavesReading()
    {
        var (service, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Toàn bộ cung chiêm tinh đi từ bản thân đến tiềm thức.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(overview));
        var request = ValidRequest();

        var result = await service.CreateTwelveHousesReadingAsync(request, userId);

        var entity = Assert.Single(db.AIDeepTarotReadings);
        entity.Id.Should().NotBeEmpty();
        result.Id.Should().Be(entity.Id);
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

    /// <summary>
    /// Every spread position key ("house-1".."house-12") is handed to the model exactly once,
    /// paired with the card drawn on that house.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_ValidRequest_PromptContainsAllHouseKeysAndCards()
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        for (var i = 1; i <= 12; i++)
        {
            rawPrompt.Should().Contain($"house-{i}");
        }
        TwelveCardCodes
            .Select(code => TarotConstant.GetCardName(code))
            .Should()
            .AllSatisfy(name => rawPrompt.Should().Contain(name));
        rawPrompt
            .Should()
            .Contain("Reversed")
            .And.Contain("Upright")
            .And.Contain("Vietnamese");
    }

    /// <summary>
    /// The requested locale reaches the prompt so the answer is written in that language.
    /// </summary>
    [Theory]
    [InlineData("vi", "Vietnamese")]
    [InlineData("en", "English")]
    public async Task CreateTwelveHousesReading_Locale_SetsPromptLanguage(string locale, string expected)
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Overview."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(locale), Guid.NewGuid());

        rawPrompt.Should().Contain(expected);
    }

    /// <summary>
    /// A long overview is truncated to the summary limit for the list excerpt.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_LongOverview_TruncatesAnswerSummary()
    {
        var (service, db, gemini, _) = CreateSut();
        var longOverview = new string('a', 800);
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(longOverview));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        entity.AnswerSummary.Should().HaveLength(500);
        entity.AnswerSummary.Should().Be(longOverview[..500]);
    }

    /// <summary>
    /// When Gemini reports a card name (e.g. "the_fool") instead of a canonical code, the stored
    /// answer's cardCode is rewritten to the matching drawn card's canonical code.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_SectionCardCodeByName_StoresCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "the_fool" })
            );

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
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
    public async Task CreateTwelveHousesReading_SectionCardCodeNameInDeck_ResolvesToCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "five_of_wands" })
            );

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("min-wands-5");
    }

    /// <summary>
    /// An unrecognizable cardCode falls back to the drawn card at the same house so the client
    /// never receives an invalid code.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_SectionCardCodeGarbage_FallsBackToDrawnCard()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "xyz-123" })
            );

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    /// <summary>
    /// A section whose key is unknown or missing is re-keyed to the spread position at the same
    /// index, so every section always maps back to a house.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_SectionKeyUnknown_FallsBackToPositionKey()
    {
        var (service, db, gemini, _) = CreateSut();
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

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        ParseAnswer(entity.Answer)
            .GetProperty("sections")[0]
            .GetProperty("key")
            .GetString()
            .Should()
            .Be("house-1");
    }

    /// <summary>
    /// When the Gemini call fails, an InternalServerException bubbles up and nothing
    /// is persisted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_GeminiFails_ThrowsInternalServerAndDoesNotSave()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InternalServerException(
                    AiDeepTarotErrorCode.GenerationFailed,
                    innerException: new Exception("boom")
                )
            );

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InternalServerException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.GenerationFailed);
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A malformed (non-JSON) Gemini response fails with InternalServerException and
    /// nothing is persisted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_InvalidGeminiJson_ThrowsInternalServerAndDoesNotSave()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("this is not json");

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<InternalServerException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.GenerationFailed);
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request whose card list length does not match the 12-house spread is rejected with
    /// BadRequestException before Gemini is ever called.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_CardCountMismatch_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with
        {
            Cards = requestCards(10),
        };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();

        static List<AiDeepCardRequest> requestCards(int count) =>
            TwelveCardCodes
                .Take(count)
                .Select(code => new AiDeepCardRequest(code, false))
                .ToList();
    }

    /// <summary>
    /// A request with a locale other than "en"/"vi" is rejected with BadRequestException.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_InvalidLocale_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Locale = "fr" };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidLocale);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request with an invalid card code is rejected with BadRequestException before
    /// Gemini is ever called.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_InvalidCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var cards = ValidRequest().Cards;
        cards[5] = new AiDeepCardRequest("fake-card", false);
        var request = ValidRequest() with { Cards = cards };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCard);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A spread drawn with the same card twice is rejected, so every house has its own card.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_DuplicateCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var cards = ValidRequest().Cards;
        cards[5] = new AiDeepCardRequest("maj-00", false);
        var request = ValidRequest() with { Cards = cards };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCard);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A malformed body with a null card list is rejected with BadRequestException rather than
    /// throwing a NullReferenceException (which would surface as a 500).
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_NullCards_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Cards = null! };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// An empty card list is rejected with BadRequestException.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_EmptyCards_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Cards = [] };

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A deep reading is charged the topic's red coin cost (3 for the 12 houses) exactly once as
    /// an AIDeepTarot order, and the reading is persisted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_ValidRequest_DeductsThreeRedCoins()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    new DeductRedCoinRequest(3, OrderType.AIDeepTarot),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AIDeepTarotReadings.Should().ContainSingle();
    }

    /// <summary>
    /// The charged amount is the one declared by the topic's spread definition.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_ValidRequest_ChargesTheTopicCost()
    {
        var (service, _, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        var cost = DeepTarotConstant.GetCost(DeepTarotTopic.TwelveHouses);
        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    It.Is<DeductRedCoinRequest>(r => r.Amount == cost),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// When the red coin balance is below the reading cost, a BadRequestException with the
    /// insufficientRedCoin code is thrown before Gemini is called, nothing is persisted and no
    /// coin is deducted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_InsufficientRedCoin_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, redCoin: 2);

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == WalletErrorCode.InsufficientRedCoin);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DeductRedCoinRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// Having exactly the required red coins is enough to read.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_ExactRedCoinBalance_Succeeds()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, DeepTarotConstant.GetCost(DeepTarotTopic.TwelveHouses));
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        db.AIDeepTarotReadings.Should().ContainSingle();
        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DeductRedCoinRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// A failed Gemini call throws before any coin is deducted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_GeminiFails_DeductsNoRedCoin()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("gemini down"));

        var act = async () =>
            await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DeductRedCoinRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    #endregion

    #region CreateTwelveHousesReadingAsync - TwelveMonths

    /// <summary>
    /// A valid 12-months request is accepted and persisted with the months topic, 12 sections
    /// and the 12 drawn cards.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_SavesReading()
    {
        var (service, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Một năm tới với 12 tháng liên tục.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson(overview, DateTimeOffset.UtcNow));

        var result = await service.CreateTwelveMonthsReadingAsync(
            ValidMonthsRequest(),
            userId
        );

        var entity = Assert.Single(db.AIDeepTarotReadings);
        entity.Topic.Should().Be(DeepTarotTopic.TwelveMonths);
        entity.UserId.Should().Be(userId);
        entity.Title.Should().Be("Vận mệnh 12 tháng tới");
        entity.AnswerSummary.Should().Be(overview);
        result.Id.Should().Be(entity.Id);
        var answer = ParseAnswer(entity.Answer);
        answer.GetProperty("overview").GetString().Should().Be(overview);
        var sections = answer.GetProperty("sections").EnumerateArray().ToList();
        sections.Should().HaveCount(12);
        sections
            .Select(s => s.GetProperty("key").GetString())
            .Should()
            .Equal(Enumerable.Range(1, 12).Select(i => $"month-{i}"));
        JsonSerializer
            .Deserialize<List<AiDeepReadingCard>>(
                entity.Cards,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )
            .Should()
            .HaveCount(12);
    }

    /// <summary>
    /// The 12-months spread is only rejected by the validator once it has no spread definition,
    /// so the topic now passes validation and reaches Gemini.
    /// </summary>
    [Fact]
    public void TwelveMonthsTopic_HasSpreadDefinition()
    {
        DeepTarotConstant.IsSupported(DeepTarotTopic.TwelveMonths).Should().BeTrue();
        DeepTarotConstant
            .GetRequiredCardCount(DeepTarotTopic.TwelveMonths)
            .Should()
            .Be(12);
        DeepTarotConstant
            .GetPositions(DeepTarotTopic.TwelveMonths)
            .Should()
            .Equal(DeepTarotConstant.TwelveMonthsPositions);
    }

    /// <summary>
    /// The months prompt carries every month position key ("month-1".."month-12") together with
    /// the resolved calendar month labels, and starts at the month after the reading month.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_PromptContainsMonthKeysAndNextMonthLabels()
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

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

    /// <summary>
    /// The first month of the spread is the month right after the reading month, and the last
    /// month is the same calendar month of the following year.
    /// </summary>
    [Fact]
    public void GetMonthLabel_FirstMonthIsNextMonthAndLastMonthIsNextYear()
    {
        var createdAt = new DateTimeOffset(2026, 9, 15, 10, 30, 0, TimeSpan.Zero);
        var positions = DeepTarotConstant.TwelveMonthsPositions;

        DeepTarotConstant.GetMonthLabel(positions[0], createdAt).Should().Be("10/2026");
        DeepTarotConstant.GetMonthLabel(positions[1], createdAt).Should().Be("11/2026");
        DeepTarotConstant.GetMonthLabel(positions[11], createdAt).Should().Be("09/2027");
    }

    /// <summary>
    /// A reading created in December rolls the spread over into the next year.
    /// </summary>
    [Fact]
    public void GetMonthLabel_ReadingInDecember_RollsIntoNextYear()
    {
        var createdAt = new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.Zero);
        var positions = DeepTarotConstant.TwelveMonthsPositions;

        DeepTarotConstant.GetMonthLabel(positions[0], createdAt).Should().Be("01/2027");
        DeepTarotConstant.GetMonthLabel(positions[11], createdAt).Should().Be("12/2027");
    }

    /// <summary>
    /// The month labels ignore the time of day, so late-evening and early-morning creations
    /// in the same month produce the same spread.
    /// </summary>
    [Fact]
    public void GetMonthLabel_IsStableAcrossTheWholeReadingMonth()
    {
        var positions = DeepTarotConstant.TwelveMonthsPositions;
        var firstInstant = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var lastInstant = new DateTimeOffset(2026, 3, 31, 23, 59, 59, TimeSpan.Zero);

        DeepTarotConstant
            .GetMonthLabel(positions[0], firstInstant)
            .Should()
            .Be(DeepTarotConstant.GetMonthLabel(positions[0], lastInstant));
        DeepTarotConstant.GetMonthLabel(positions[0], firstInstant).Should().Be("04/2026");
    }

    /// <summary>
    /// The months spread is charged the same 3 red coins as the houses spread.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_DeductsThreeRedCoins()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

        DeepTarotConstant.GetCost(DeepTarotTopic.TwelveMonths).Should().Be(3);
        wallet.Verify(
            w =>
                w.DeductRedCoinAsync(
                    It.IsAny<Guid>(),
                    new DeductRedCoinRequest(3, OrderType.AIDeepTarot),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        db.AIDeepTarotReadings.Should().ContainSingle();
    }

    /// <summary>
    /// Having exactly 3 red coins is enough to read the months spread.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_ExactRedCoinBalance_Succeeds()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, DeepTarotConstant.GetCost(DeepTarotTopic.TwelveMonths));
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

        db.AIDeepTarotReadings.Should().ContainSingle();
    }

    /// <summary>
    /// Below 3 red coins the months spread is rejected before Gemini is called and nothing
    /// is persisted.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_InsufficientRedCoin_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, redCoin: 2);

        var act = async () =>
            await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == WalletErrorCode.InsufficientRedCoin);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// The months spread is validated by its own validator: a card list that does not match the
    /// 12-month spread is rejected with BadRequestException before Gemini is ever called.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_CardCountMismatch_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidMonthsRequest() with { Cards = Cards(10, reversalStep: 3) };

        var act = async () =>
            await service.CreateTwelveMonthsReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCardCount);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// The months spread rejects a repeated card so every month gets its own card.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_DuplicateCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var cards = ValidMonthsRequest().Cards;
        cards[5] = new AiDeepCardRequest("maj-00", false);
        var request = ValidMonthsRequest() with { Cards = cards };

        var act = async () =>
            await service.CreateTwelveMonthsReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidCard);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// The months spread rejects an unsupported locale with BadRequestException.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_InvalidLocale_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidMonthsRequest() with { Locale = "fr" };

        var act = async () =>
            await service.CreateTwelveMonthsReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.InvalidLocale);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// The months prompt must not leak the astrological 12-house instructions into the
    /// month-by-month spread.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_PromptDoesNotMentionHouses()
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildMonthsGeminiJson("Tổng quan.", DateTimeOffset.UtcNow));

        await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

        rawPrompt.Should().NotContain("house-");
        rawPrompt.Should().NotContain("astrological");
        rawPrompt.Should().Contain("MM/yyyy");
    }

    /// <summary>
    /// The houses prompt keeps its astrological wording and carries no calendar month labels.
    /// </summary>
    [Fact]
    public async Task CreateTwelveHousesReading_PromptDoesNotMentionMonths()
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateTwelveHousesReadingAsync(ValidRequest(), Guid.NewGuid());

        rawPrompt.Should().NotContain("month-");
        rawPrompt.Should().NotContain("MM/yyyy");
        rawPrompt.Should().Contain("astrological");
    }

    /// <summary>
    /// A Gemini section with an unknown key falls back to the positional month key, so the
    /// months spread always comes back with 12 month-* sections.
    /// </summary>
    [Fact]
    public async Task CreateTwelveMonthsReading_SectionKeyUnknown_FallsBackToMonthKey()
    {
        var (service, db, gemini, _) = CreateSut();
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

        await service.CreateTwelveMonthsReadingAsync(ValidMonthsRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        var sections = ParseAnswer(entity.Answer).GetProperty("sections").EnumerateArray().ToList();
        sections
            .Select(s => s.GetProperty("key").GetString())
            .Should()
            .Equal(Enumerable.Range(1, 12).Select(i => $"month-{i}"));
    }

    #endregion

    #region GetAiDeepTarotReadingByIdAsync

    /// <summary>
    /// An existing reading for the user is returned with the full answer and 12 cards.
    /// </summary>
    [Fact]
    public async Task GetAiDeepTarotReading_Exists_ReturnsFullReading()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        var result = await service.GetAiDeepTarotReadingByIdAsync(userId, reading.Id);

        result.Id.Should().Be(reading.Id);
        result.Topic.Should().Be(DeepTarotTopic.TwelveHouses);
        result.Title.Should().Be(reading.Title);
        var answer = ParseAnswer(result.Answer);
        answer.GetProperty("sections").GetArrayLength().Should().Be(12);
        answer.GetProperty("overview").GetString().Should().Contain("12 nhà");
        result.Cards.Should().ContainSingle();
        result.Cards[0].CardCode.Should().Be("maj-00");
        result.Cards[0].IsReversed.Should().BeFalse();
    }

    /// <summary>
    /// An already-stored answer whose section cardCode is a card name is normalized to a
    /// canonical code when returned.
    /// </summary>
    [Fact]
    public async Task GetAiDeepTarotReading_StoredAnswerWithNameCode_ReturnsCanonicalCode()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = DeepTarotTopic.TwelveHouses,
            Title = "Vận mệnh qua 12 nhà",
            Answer = BuildGeminiJson(
                "Một tổng quan đầy đủ.",
                new Dictionary<int, string> { [1] = "the_fool" }
            ),
            AnswerSummary = "Một tổng quan đầy đủ.",
            Cards = """[{"cardCode":"maj-00","isReversed":false}]""",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.AIDeepTarotReadings.Add(reading);
        await db.SaveChangesAsync();

        var result = await service.GetAiDeepTarotReadingByIdAsync(userId, reading.Id);

        ParseAnswer(result.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    /// <summary>
    /// A reading id that does not exist throws NotFoundException with the
    /// AiDeepTarotErrorCode.ReadingNotFound code.
    /// </summary>
    [Fact]
    public async Task GetAiDeepTarotReading_NotFound_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();
        var userId = Guid.NewGuid();

        var act = async () =>
            await service.GetAiDeepTarotReadingByIdAsync(userId, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A reading that exists but belongs to a different user is treated as not found.
    /// </summary>
    [Fact]
    public async Task GetAiDeepTarotReading_BelongsToOtherUser_ThrowsNotFound()
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

        var act = async () =>
            await service.GetAiDeepTarotReadingByIdAsync(userA, readingB.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A soft-deleted reading is excluded by the global query filter and treated as not found.
    /// </summary>
    [Fact]
    public async Task GetAiDeepTarotReading_SoftDeleted_ThrowsNotFound()
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

        var act = async () =>
            await service.GetAiDeepTarotReadingByIdAsync(userId, reading.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.ReadingNotFound);
    }

    #endregion

    #region GetAllAiDeepTarotReadingsAsync

    /// <summary>
    /// The list result maps every reading into an item with only the short excerpt —
    /// the full Answer is never exposed on the list DTO.
    /// </summary>
    [Fact]
    public async Task GetAllAiDeepTarotReadings_HasReadings_ReturnsItemsWithoutFullAnswer()
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

        var result = await service.GetAllAiDeepTarotReadingsAsync(userId);

        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(newest.Id);
        result.Items[0].Topic.Should().Be(DeepTarotTopic.TwelveHouses);
        result.Items[0].AnswerSummary.Should().Be("Một tổng quan đầy đủ về cả 12 nhà.");
        result.Items[0].Cards.Should().ContainSingle(x => x.CardCode == "maj-00");
        typeof(GetAllAiDeepTarotReadingItem)
            .GetProperty("Answer")
            .Should()
            .BeNull("the list DTO must not carry the full answer");
    }

    /// <summary>
    /// A user with no readings gets back an empty (non-null) list.
    /// </summary>
    [Fact]
    public async Task GetAllAiDeepTarotReadings_NoReadings_ReturnsEmptyList()
    {
        var (service, _, _, _) = CreateSut();

        var result = await service.GetAllAiDeepTarotReadingsAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Only the requested user's readings are returned, never another user's.
    /// </summary>
    [Fact]
    public async Task GetAllAiDeepTarotReadings_OnlyReturnsOwnReadings()
    {
        var (service, db, _, _) = CreateSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        await SeedReadingAsync(db, userA, """[{"cardCode":"maj-00","isReversed":false}]""");
        await SeedReadingAsync(db, userB, """[{"cardCode":"maj-21","isReversed":true}]""");

        var result = await service.GetAllAiDeepTarotReadingsAsync(userA);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Cards.Single().CardCode.Should().Be("maj-00");
    }

    /// <summary>
    /// Soft-deleted readings are excluded by the global query filter.
    /// </summary>
    [Fact]
    public async Task GetAllAiDeepTarotReadings_ExcludesSoftDeletedRecords()
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

        var result = await service.GetAllAiDeepTarotReadingsAsync(userId);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Cards.Single().CardCode.Should().Be("maj-00");
    }

    /// <summary>
    /// Readings are returned newest-first (ordered by CreatedAt descending).
    /// </summary>
    [Fact]
    public async Task GetAllAiDeepTarotReadings_ReturnsOrderedByCreatedAtDescending()
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

        var result = await service.GetAllAiDeepTarotReadingsAsync(userId);

        result.Items.Select(x => x.Id).Should().Equal(newest.Id, oldest.Id);
    }

    #endregion

    #region DeleteAiDeepTarotReadingAsync

    /// <summary>
    /// Deleting an existing reading soft-deletes it (DeletedAt set) and makes it
    /// disappear from further GetAllAiDeepTarotReadingsAsync results.
    /// </summary>
    [Fact]
    public async Task DeleteAiDeepTarotReading_ExistingReading_SoftDeletes()
    {
        var (service, db, _, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var reading = await SeedReadingAsync(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]"""
        );

        await service.DeleteAiDeepTarotReadingAsync(userId, reading.Id);

        db.AIDeepTarotReadings
            .IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        var afterDelete = await service.GetAllAiDeepTarotReadingsAsync(userId);
        afterDelete.Items.Should().BeEmpty();
    }

    /// <summary>
    /// A reading id that does not exist throws NotFoundException with the
    /// AiDeepTarotErrorCode.ReadingNotFound code.
    /// </summary>
    [Fact]
    public async Task DeleteAiDeepTarotReading_ReadingNotFound_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();
        var userId = Guid.NewGuid();

        var act = async () =>
            await service.DeleteAiDeepTarotReadingAsync(userId, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.ReadingNotFound);
    }

    /// <summary>
    /// A reading that exists but belongs to a different user is treated as not found
    /// (NotFoundException) and the other user's reading is left untouched.
    /// </summary>
    [Fact]
    public async Task DeleteAiDeepTarotReading_BelongsToOtherUser_ThrowsNotFound()
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

        var act = async () => await service.DeleteAiDeepTarotReadingAsync(userA, readingB.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.ReadingNotFound);
        db.AIDeepTarotReadings
            .IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }

    #endregion
}
