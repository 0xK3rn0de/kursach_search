using Core.Preprocessing;
using FluentAssertions;

namespace Tests;

public class TokenizerTests
{
    private readonly Tokenizer _tokenizer = new();

    [Fact]
    public void Tokenize_SimpleText_ReturnsCorrectTokens()
    {
        // Arrange
        var text = "Python разработчик Django";

        // Act
        var tokens = _tokenizer.Tokenize(text);

        // Assert
        tokens.Should().HaveCount(3);
        tokens.Should().ContainInOrder("Python", "разработчик", "Django");
    }

    [Fact]
    public void Tokenize_TextWithSpecialChars_PreservesValidTokens()
    {
        // Arrange
        var text = "C# .NET ASP.NET Core";

        // Act
        var tokens = _tokenizer.Tokenize(text);

        // Assert
        tokens.Should().Contain("C#");
        tokens.Should().Contain(".NET");
        tokens.Should().Contain("ASP.NET");
    }

    [Fact]
    public void Tokenize_TextWithPunctuation_IgnoresPunctuation()
    {
        // Arrange
        var text = "Вакансия: разработчик, опыт — 3 года!";

        // Act
        var tokens = _tokenizer.Tokenize(text);

        // Assert
        tokens.Should().Contain("Вакансия");
        tokens.Should().Contain("разработчик");
        tokens.Should().Contain("опыт");
        tokens.Should().Contain("3");
        tokens.Should().Contain("года");
        tokens.Should().NotContain(":");
        tokens.Should().NotContain(",");
        tokens.Should().NotContain("!");
    }

    [Fact]
    public void Tokenize_EmptyString_ReturnsEmptyList()
    {
        // Arrange & Act
        var tokens = _tokenizer.Tokenize("");

        // Assert
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Tokenize_NullString_ReturnsEmptyList()
    {
        // Arrange & Act
        var tokens = _tokenizer.Tokenize(null!);

        // Assert
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Tokenize_WhitespaceOnly_ReturnsEmptyList()
    {
        // Arrange & Act
        var tokens = _tokenizer.Tokenize("   \t\n  ");

        // Assert
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void TokenizeWithPositions_ReturnsCorrectPositions()
    {
        // Arrange
        var text = "Python Django Flask";

        // Act
        var result = _tokenizer.TokenizeWithPositions(text);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be(("Python", 0));
        result[1].Should().Be(("Django", 1));
        result[2].Should().Be(("Flask", 2));
    }

    [Theory]
    [InlineData("Node.js", "Node.js")]
    [InlineData("C#", "C#")]
    [InlineData("ASP.NET", "ASP.NET")]
    public void Tokenize_TechnologyNames_HandledCorrectly(string input, string expected)
    {
        // Act
        var tokens = _tokenizer.Tokenize(input);

        // Assert
        tokens.Should().Contain(expected);
    }
}