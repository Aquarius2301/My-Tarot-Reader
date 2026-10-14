using FluentAssertions;
using MyTarotReader.Application.Features.Tarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class CreateDrawForAuthCommandHandlerTests
{
    private const string ValidCard = "maj-00";

    private static CreateDrawForAuthCommandHandler CreateSut(AppDbContext db) => new(db);

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

    [Fact]
    public async Task CreateDrawForAuth_Valid_StoresForUser()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new CreateDrawForAuthCommand(ValidCard, true, userId), CancellationToken.None);

        // Assert
        var entity = db.TarotReadings.Should().ContainSingle().Subject;
        entity.UserId.Should().Be(userId);
        entity.CardCode.Should().Be(ValidCard);
        entity.IsReversed.Should().BeTrue();
    }
}
