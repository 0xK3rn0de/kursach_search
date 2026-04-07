using Core.Preprocessing;
using FluentAssertions;

namespace Tests;

public class TextNormalizerTests
{
    private readonly TextNormalizer _normalizer = new();

    [Theory]
    [InlineData("Python", "python")]
    [InlineData("DJANGO", "django")]
    [InlineData("JavaScript", "javascript")]
    [InlineData("C#", "c#")]
    public void Normalize_MixedCase_ReturnsLowercase(string input, string expected)
    {
        // Act
        var result = _normalizer.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Normalize_WithTrailingDot_RemovesDot()
    {
        // Arrange
        var token = "etc.";

        // Act
        var result = _normalizer.Normalize(token);

        // Assert
        result.Should().Be("etc");
    }

    [Fact]
    public void NormalizeAll_ListOfTokens_NormalizesAll()
    {
        // Arrange
        var tokens = new List<string> { "Python", "DJANGO", "Flask." };

        // Act
        var result = _normalizer.NormalizeAll(tokens);

        // Assert
        result.Should().ContainInOrder("python", "django", "flask");
    }

    [Fact]
    public void NormalizeAll_EmptyList_ReturnsEmptyList()
    {
        // Act
        var result = _normalizer.NormalizeAll(new List<string>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_CyrillicText_HandlesCorrectly()
    {
        // Arrange
        var token = "Разработчик";

        // Act
        var result = _normalizer.Normalize(token);

        // Assert
        result.Should().Be("разработчик");
    }
}
