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
            new CreateAiDeepTarotReadingRequestValidator()
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

    private static CreateAiDeepTarotReadingRequest ValidRequest(string locale = "vi") =>
        new(
            DeepTarotTopic.TwelveHouses,
            locale,
            TwelveCardCodes
                .Select((code, index) => new AiDeepCardRequest(code, index % 3 == 0))
                .ToList()
        );

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

    #endregion

    #region CreateAiDeepTarotReadingAsync

    /// <summary>
    /// A valid 12-house request with a successful Gemini response persists the reading
    /// (topic, title, full answer, answer summary, 12 cards) for the correct user.
    /// </summary>
    [Fact]
    public async Task CreateAiDeepTarotReading_ValidRequest_SavesReading()
    {
        var (service, db, gemini, _) = CreateSut();
        var userId = Guid.NewGuid();
        var overview = "Toàn bộ cung chiêm tinh đi từ bản thân đến tiềm thức.";
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(overview));
        var request = ValidRequest();

        var result = await service.CreateAiDeepTarotReadingAsync(request, userId);

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
    public async Task CreateAiDeepTarotReading_ValidRequest_PromptContainsAllHouseKeysAndCards()
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_Locale_SetsPromptLanguage(string locale, string expected)
    {
        var (service, _, gemini, _) = CreateSut();
        var rawPrompt = string.Empty;
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => rawPrompt = prompt)
            .ReturnsAsync(BuildGeminiJson("Overview."));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(locale), Guid.NewGuid());

        rawPrompt.Should().Contain(expected);
    }

    /// <summary>
    /// A long overview is truncated to the summary limit for the list excerpt.
    /// </summary>
    [Fact]
    public async Task CreateAiDeepTarotReading_LongOverview_TruncatesAnswerSummary()
    {
        var (service, db, gemini, _) = CreateSut();
        var longOverview = new string('a', 800);
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson(longOverview));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

        var entity = Assert.Single(db.AIDeepTarotReadings);
        entity.AnswerSummary.Should().HaveLength(500);
        entity.AnswerSummary.Should().Be(longOverview[..500]);
    }

    /// <summary>
    /// When Gemini reports a card name (e.g. "the_fool") instead of a canonical code, the stored
    /// answer's cardCode is rewritten to the matching drawn card's canonical code.
    /// </summary>
    [Fact]
    public async Task CreateAiDeepTarotReading_SectionCardCodeByName_StoresCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "the_fool" })
            );

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_SectionCardCodeNameInDeck_ResolvesToCanonicalCode()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "five_of_wands" })
            );

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_SectionCardCodeGarbage_FallsBackToDrawnCard()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                BuildGeminiJson("Tổng quan.", new Dictionary<int, string> { [1] = "xyz-123" })
            );

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_SectionKeyUnknown_FallsBackToPositionKey()
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

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_GeminiFails_ThrowsInternalServerAndDoesNotSave()
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
            await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_InvalidGeminiJson_ThrowsInternalServerAndDoesNotSave()
    {
        var (service, db, gemini, _) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("this is not json");

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_CardCountMismatch_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with
        {
            Cards = requestCards(10),
        };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    /// A topic that has no spread definition yet is rejected with BadRequestException
    /// (topicNotSupported) before Gemini is ever called.
    /// </summary>
    [Theory]
    [InlineData(DeepTarotTopic.TwelveMonths)]
    [InlineData(DeepTarotTopic.LoveBetweenTwo)]
    [InlineData(DeepTarotTopic.Crossroads)]
    public async Task CreateAiDeepTarotReading_UnsupportedTopic_ThrowsBadRequestAndSkipsGemini(
        DeepTarotTopic topic
    )
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Topic = topic };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AiDeepTarotErrorCode.TopicNotSupported);
        gemini.Verify(
            g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// A request with a locale other than "en"/"vi" is rejected with BadRequestException.
    /// </summary>
    [Fact]
    public async Task CreateAiDeepTarotReading_InvalidLocale_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Locale = "fr" };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_InvalidCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var cards = ValidRequest().Cards;
        cards[5] = new AiDeepCardRequest("fake-card", false);
        var request = ValidRequest() with { Cards = cards };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_DuplicateCard_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var cards = ValidRequest().Cards;
        cards[5] = new AiDeepCardRequest("maj-00", false);
        var request = ValidRequest() with { Cards = cards };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_NullCards_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Cards = null! };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_EmptyCards_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, _) = CreateSut();
        var request = ValidRequest() with { Cards = [] };

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(request, Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_ValidRequest_DeductsThreeRedCoins()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_ValidRequest_ChargesTheTopicCost()
    {
        var (service, _, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_InsufficientRedCoin_ThrowsBadRequestAndSkipsGemini()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, redCoin: 2);

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_ExactRedCoinBalance_Succeeds()
    {
        var (service, db, gemini, wallet) = CreateSut();
        SetupRedCoinBalance(wallet, DeepTarotConstant.GetCost(DeepTarotTopic.TwelveHouses));
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGeminiJson("Tổng quan."));

        await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
    public async Task CreateAiDeepTarotReading_GeminiFails_DeductsNoRedCoin()
    {
        var (service, db, gemini, wallet) = CreateSut();
        gemini
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("gemini down"));

        var act = async () =>
            await service.CreateAiDeepTarotReadingAsync(ValidRequest(), Guid.NewGuid());

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
