using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Features.AiDeepTarot;
using MyTarotReader.Domain.Enums;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class CreateCrossroadsReadingCommandValidatorTests
{
    private readonly CreateCrossroadsReadingCommandValidator _sut = new();

    private static readonly string[] DefaultOptions =
        ["Ở lại công ty hiện tại", "Nhảy sang công ty mới"];

    private static List<AiDeepCardRequest> Cards(int count) =>
        Enumerable.Range(0, count).Select(i => new AiDeepCardRequest($"maj-{i:D2}", false)).ToList();

    private static CreateCrossroadsReadingCommand ValidCommand(
        string[]? options = null,
        string? locale = "vi"
    )
    {
        var resolvedOptions = options ?? DefaultOptions;

        return new CreateCrossroadsReadingCommand(
            locale!,
            "Tôi nên chọn công việc nào?",
            [.. resolvedOptions],
            CrossroadsTimeFrame.OneToThreeMonths,
            Cards(DeepTarotConstant.GetCrossroadsCardCount(resolvedOptions.Length)),
            Guid.NewGuid()
        );
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        // Act
        var result = _sut.Validate(ValidCommand());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public void Validate_SupportedLocale_IsValid(string locale)
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale: locale));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullLocale_IsValid()
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale: null));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UnsupportedLocale_IsInvalidWithInvalidLocale()
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale: "fr"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateCrossroadsReadingCommand.Locale))
            .Which.ErrorMessage.Should()
            .Be(AiDeepTarotErrorCode.InvalidLocale);
    }

    [Fact]
    public void Validate_EmptyQuestion_IsInvalidWithInvalidQuestion()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { Question = "   " });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateCrossroadsReadingCommand.Question))
            .Which.ErrorMessage.Should()
            .Be(AiDeepTarotErrorCode.InvalidQuestion);
    }

    [Fact]
    public void Validate_QuestionTooLong_IsInvalidWithInvalidQuestion()
    {
        // Act
        var result = _sut.Validate(
            ValidCommand() with
            {
                Question = new string('a', DeepTarotConstant.CrossroadsQuestionMaxLength + 1),
            }
        );

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Question)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidQuestion
            );
    }

    [Fact]
    public void Validate_TooFewOptions_IsInvalidWithInvalidOption()
    {
        // Arrange
        var options = new[] { "Chỉ một lựa chọn" };

        // Act
        var result = _sut.Validate(ValidCommand(options));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Options)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidOption
            );
    }

    [Fact]
    public void Validate_TooManyOptions_IsInvalidWithInvalidOption()
    {
        // Arrange
        var options = new[] { "A", "B", "C", "D", "E" };

        // Act
        var result = _sut.Validate(ValidCommand(options));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Options)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidOption
            );
    }

    [Fact]
    public void Validate_EmptyOption_IsInvalidWithInvalidOption()
    {
        // Act
        var result = _sut.Validate(ValidCommand(["", "Sống tiếp"]));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Options)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidOption
            );
    }

    [Fact]
    public void Validate_OptionTooLong_IsInvalidWithInvalidOption()
    {
        // Arrange
        var tooLong = new string('a', DeepTarotConstant.CrossroadsOptionMaxLength + 1);

        // Act
        var result = _sut.Validate(ValidCommand([tooLong, "Sống tiếp"]));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Options)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidOption
            );
    }

    [Fact]
    public void Validate_InvalidTimeFrame_IsInvalidWithInvalidTimeFrame()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { TimeFrame = (CrossroadsTimeFrame)99 });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateCrossroadsReadingCommand.TimeFrame))
            .Which.ErrorMessage.Should()
            .Be(AiDeepTarotErrorCode.InvalidTimeFrame);
    }

    [Fact]
    public void Validate_NullTimeFrame_IsValid()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { TimeFrame = null });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CardCountMismatch_IsInvalidWithInvalidCardCount()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { Cards = Cards(3) });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Cards)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidCardCount
            );
    }

    [Fact]
    public void Validate_InvalidCardCode_IsInvalidWithInvalidCard()
    {
        // Arrange
        var cards = Cards(7);
        cards[4] = new AiDeepCardRequest("fake-card", false);

        // Act
        var result = _sut.Validate(ValidCommand() with { Cards = cards });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateCrossroadsReadingCommand.Cards)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidCard
            );
    }
}
