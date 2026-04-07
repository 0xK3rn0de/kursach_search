using Core.Preprocessing;
using FluentAssertions;

namespace Tests;

public class TextPreprocessorTests
{
    [Fact]
    public void Process_FullPipeline_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new TextPreprocessor(removeStopWords: true);
        var text = "Ищем Python разработчика в команду Django";

        // Act
        var result = preprocessor.Process(text);

        // Assert
        result.Should().Contain("python");
        result.Should().Contain("разработчика");
        result.Should().Contain("django");
        result.Should().NotContain("в");  // стоп-слово
        result.Should().NotContain("Ищем"); // должно быть в lowercase
        result.All(t => t == t.ToLowerInvariant()).Should().BeTrue();
    }

    [Fact]
    public void Process_WithoutStopWordsFilter_KeepsStopWords()
    {
        // Arrange
        var preprocessor = new TextPreprocessor(removeStopWords: false);
        var text = "Работа в Москве";

        // Act
        var result = preprocessor.Process(text);

        // Assert
        result.Should().Contain("в");
    }

    [Fact]
    public void ProcessWithPositions_ReturnsCorrectPositions()
    {
        // Arrange
        var preprocessor = new TextPreprocessor(removeStopWords: true);
        var text = "Python и Django";

        // Act
        var result = preprocessor.ProcessWithPositions(text);

        // Assert
        result.Should().HaveCount(2); // "и" отфильтровано
        result.Should().Contain(("python", 0));
        result.Should().Contain(("django", 2)); // позиция сохранена оригинальная
    }

    [Fact]
    public void Process_EmptyText_ReturnsEmptyList()
    {
        // Arrange
        var preprocessor = new TextPreprocessor();

        // Act
        var result = preprocessor.Process("");

        // Assert
        result.Should().BeEmpty();
    }
}
