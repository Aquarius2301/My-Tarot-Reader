using FluentAssertions;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Domain.Enums;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class DeepTarotConstantTests
{
    [Theory]
    [InlineData(2, 7)]
    [InlineData(3, 10)]
    [InlineData(4, 13)]
    public void CrossroadsSpread_CardCountAndKeysFollowTheOptionCount(int optionCount, int expectedCards)
    {
        // Act
        var positions = DeepTarotConstant.GetCrossroadsPositions(optionCount);

        // Assert
        positions.Should().HaveCount(expectedCards);
        DeepTarotConstant.GetCrossroadsCardCount(optionCount).Should().Be(expectedCards);
        positions
            .Select((position, index) => position.Number)
            .Should()
            .Equal(Enumerable.Range(1, expectedCards));
        positions
            .Take(optionCount * 3)
            .Select(p => p.Key)
            .Should()
            .AllSatisfy(key =>
                key.Should().MatchRegex(@"^option-[1-4]-(current|evolution|outcome)$")
            );
        positions[0].Key.Should().Be("option-1-current");
        positions[2].Key.Should().Be("option-1-outcome");
        positions[3].Key.Should().Be("option-2-current");
        positions[^1].Key.Should().Be("summary");
        positions[^1].Number.Should().Be(expectedCards);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    public void CrossroadsSpread_CostIsOneRedCoinPerOption(int optionCount, int expectedCost)
    {
        // Act
        var cost = DeepTarotConstant.GetCost(DeepTarotTopic.Crossroads, optionCount);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Fact]
    public void TwelveMonthsTopic_HasSpreadDefinition()
    {
        // Assert
        DeepTarotConstant.IsSupported(DeepTarotTopic.TwelveMonths).Should().BeTrue();
        DeepTarotConstant.GetRequiredCardCount(DeepTarotTopic.TwelveMonths).Should().Be(12);
        DeepTarotConstant
            .GetPositions(DeepTarotTopic.TwelveMonths)
            .Should()
            .Equal(DeepTarotConstant.TwelveMonthsPositions);
    }

    [Fact]
    public void GetMonthLabel_FirstMonthIsNextMonthAndLastMonthIsNextYear()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2026, 9, 15, 10, 30, 0, TimeSpan.Zero);
        var positions = DeepTarotConstant.TwelveMonthsPositions;

        // Act & Assert
        DeepTarotConstant.GetMonthLabel(positions[0], createdAt).Should().Be("10/2026");
        DeepTarotConstant.GetMonthLabel(positions[1], createdAt).Should().Be("11/2026");
        DeepTarotConstant.GetMonthLabel(positions[11], createdAt).Should().Be("09/2027");
    }

    [Fact]
    public void GetMonthLabel_ReadingInDecember_RollsIntoNextYear()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.Zero);
        var positions = DeepTarotConstant.TwelveMonthsPositions;

        // Act & Assert
        DeepTarotConstant.GetMonthLabel(positions[0], createdAt).Should().Be("01/2027");
        DeepTarotConstant.GetMonthLabel(positions[11], createdAt).Should().Be("12/2027");
    }

    [Fact]
    public void GetMonthLabel_IsStableAcrossTheWholeReadingMonth()
    {
        // Arrange
        var positions = DeepTarotConstant.TwelveMonthsPositions;
        var firstInstant = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var lastInstant = new DateTimeOffset(2026, 3, 31, 23, 59, 59, TimeSpan.Zero);

        // Act & Assert
        DeepTarotConstant
            .GetMonthLabel(positions[0], firstInstant)
            .Should()
            .Be(DeepTarotConstant.GetMonthLabel(positions[0], lastInstant));
        DeepTarotConstant.GetMonthLabel(positions[0], firstInstant).Should().Be("04/2026");
    }
}
