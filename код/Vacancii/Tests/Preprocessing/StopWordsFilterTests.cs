using Core.Preprocessing;
using FluentAssertions;

namespace Tests;

public class StopWordsFilterTests
{
    private readonly StopWordsFilter _filter = new();

    [Theory]
    [InlineData("и")]
    [InlineData("в")]
    [InlineData("the")]
    [InlineData("a")]
    [InlineData("is")]
    [InlineData("для")]
    [InlineData("от")]
    public void IsStopWord_CommonStopWords_ReturnsTrue(string word)
    {
        // Act & Assert
        _filter.IsStopWord(word).Should().BeTrue();
    }

    [Theory]
    [InlineData("python")]
    [InlineData("разработчик")]
    [InlineData("django")]
    [InlineData("senior")]
    public void IsStopWord_MeaningfulWords_ReturnsFalse(string word)
    {
        // Act & Assert
        _filter.IsStopWord(word).Should().BeFalse();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("я")]
    [InlineData("x")]
    public void IsStopWord_SingleCharacter_ReturnsTrue(string word)
    {
        // Act & Assert
        _filter.IsStopWord(word).Should().BeTrue();
    }

    [Fact]
    public void Filter_MixedList_RemovesOnlyStopWords()
    {
        // Arrange
        var tokens = new List<string>
            {
                "ищем", "опытного", "python", "разработчика", "в", "команду", "и", "для", "работы"
            };

        // Act
        var result = _filter.Filter(tokens);

        // Assert
        result.Should().Contain("python");
        result.Should().Contain("разработчика");
        result.Should().Contain("ищем");
        result.Should().NotContain("в");
        result.Should().NotContain("и");
        result.Should().NotContain("для");
    }

    [Fact]
    public void Filter_EmptyList_ReturnsEmptyList()
    {
        // Act
        var result = _filter.Filter(new List<string>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Filter_OnlyStopWords_ReturnsEmptyList()
    {
        // Arrange
        var tokens = new List<string> { "и", "в", "на", "the", "a", "is" };

        // Act
        var result = _filter.Filter(tokens);

        // Assert
        result.Should().BeEmpty();
    }
}
