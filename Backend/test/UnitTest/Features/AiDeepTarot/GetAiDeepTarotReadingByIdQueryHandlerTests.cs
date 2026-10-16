using System.Text.Json;
using FluentAssertions;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Features.AiDeepTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class GetAiDeepTarotReadingByIdQueryHandlerTests
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

    private static GetAiDeepTarotReadingByIdQueryHandler CreateSut(AppDbContext db) => new(db);

    private static void SeedUser(AppDbContext db, Guid userId)
    {
        db.Users.Add(
            new User
            {
                Id = userId,
                FullName = $"User-{userId:N}",
                Email = $"user-{userId:N}@example.com",
                Picture = "http://pic",
                ProviderKey = $"provider-{userId:N}",
                Role = UserRole.Registered,
            }
        );
        db.SaveChanges();
    }

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

    private static AIDeepTarotReading SeedReading(
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
        db.SaveChanges();
        return reading;
    }

    private static JsonElement ParseAnswer(string answer) =>
        JsonDocument.Parse(answer).RootElement;

    [Fact]
    public async Task Handle_Exists_ReturnsFullReading()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiDeepTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
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

    [Fact]
    public async Task Handle_StoredAnswerWithNameCode_ReturnsCanonicalCode()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
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
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiDeepTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        ParseAnswer(result.Answer)
            .GetProperty("sections")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    [Fact]
    public async Task Handle_TwelveHouses_ReturnsNullCrossroadsFields()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiDeepTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        result.Question.Should().BeNull();
        result.Options.Should().BeNull();
        result.TimeFrame.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Crossroads_ReturnsInputsAndFullSpread()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var options = new List<string> { "A", "B", "C" };
        var positions = DeepTarotConstant.GetCrossroadsPositions(options.Count);
        var cards = Enumerable
            .Range(0, positions.Count)
            .Select(i => new AiDeepReadingCard($"maj-{i:D2}", false))
            .ToList();

        var answer = JsonSerializer.Serialize(
            new
            {
                title = "Ngã rẽ quyết định",
                overview = "Tổng quan.",
                sections = positions
                    .Select(
                        (position, index) =>
                            new
                            {
                                key = position.Key,
                                title = position.Name,
                                cardCode = cards[index].CardCode,
                                interpretation = "Diễn giải.",
                            }
                    )
                    .ToArray(),
                overallAdvice = "Lời khuyên.",
            }
        );

        var reading = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = DeepTarotTopic.Crossroads,
            Title = "Ngã rẽ quyết định",
            Answer = answer,
            AnswerSummary = "Tổng quan.",
            Cards = JsonSerializer.Serialize(cards),
            Question = "Tôi nên chọn công việc nào?",
            Options = JsonSerializer.Serialize(options),
            TimeFrame = CrossroadsTimeFrame.OneToThreeMonths,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.AIDeepTarotReadings.Add(reading);
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiDeepTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        result.Topic.Should().Be(DeepTarotTopic.Crossroads);
        result.Question.Should().Be("Tôi nên chọn công việc nào?");
        result.Options.Should().Equal(options);
        result.TimeFrame.Should().Be(CrossroadsTimeFrame.OneToThreeMonths);
        result.Cards.Should().HaveCount(10);
        ParseAnswer(result.Answer)
            .GetProperty("sections")
            .EnumerateArray()
            .Should()
            .HaveCount(10);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiDeepTarotReadingByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.ReadingNotFound);
    }

    [Fact]
    public async Task Handle_BelongsToOtherUser_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUser(db, userA);
        SeedUser(db, userB);
        var readingB = SeedReading(db, userB, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiDeepTarotReadingByIdQuery(userA, readingB.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.ReadingNotFound);
    }

    [Fact]
    public async Task Handle_SoftDeleted_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]""",
            deleted: true
        );
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiDeepTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.ReadingNotFound);
    }
}
