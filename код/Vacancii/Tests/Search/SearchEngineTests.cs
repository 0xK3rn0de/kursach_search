using Core.Indexing;
using Core.Models;
using Core.Search;
using FluentAssertions;

namespace Tests;

public class SearchEngineTests
{
    private static SearchEngine CreateEngineWithTestData()
    {
        var vacancies = new List<Vacancy>
            {
                new() { DocId = 1, Title = "Senior Python разработчик", Company = "Яндекс", Description = "Django Flask PostgreSQL", Location = "Москва", Employment = "Полная занятость", Tags = new List<string> { "python", "django" } },
                new() { DocId = 2, Title = "Middle Java разработчик", Company = "Сбер", Description = "Spring Boot Kafka", Location = "Москва", Employment = "Полная занятость", Tags = new List<string> { "java", "spring" } },
                new() { DocId = 3, Title = "Junior Python разработчик", Company = "VK", Description = "Django стажировка", Location = "Санкт-Петербург", Employment = "Стажировка", Tags = new List<string> { "python", "junior" } },
                new() { DocId = 4, Title = "Senior Java разработчик", Company = "Тинькофф", Description = "Microservices Kafka", Location = "Москва", Employment = "Удалённая работа", Tags = new List<string> { "java", "senior" } },
                new() { DocId = 5, Title = "Fullstack Python Java", Company = "Авито", Description = "Python Java React", Location = "Москва", Employment = "Полная занятость", Tags = new List<string> { "python", "java", "fullstack" } }
            };

        var indexer = new SPIMIIndexer();
        var index = indexer.BuildIndex(vacancies);

        var engine = new SearchEngine();
        engine.LoadIndex(index);

        return engine;
    }

    [Fact]
    public void Search_SimpleQuery_FindsResults()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python", useSimpleMode: true);

        // Assert
        response.Results.Should().NotBeEmpty();
        response.Results.Should().OnlyContain(r =>
            r.Vacancy.Title.Contains("Python", StringComparison.OrdinalIgnoreCase) ||
            r.Vacancy.Description.Contains("python", StringComparison.OrdinalIgnoreCase) ||
            r.Vacancy.Tags.Contains("python")
        );
    }

    [Fact]
    public void Search_BooleanAndQuery_ReturnsIntersection()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python AND java", useSimpleMode: false);

        // Assert
        response.Results.Should().HaveCount(1);
        response.Results[0].Vacancy.DocId.Should().Be(5); // Fullstack
    }

    [Fact]
    public void Search_BooleanOrQuery_ReturnsUnion()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("django OR kafka", useSimpleMode: false);

        // Assert
        response.Results.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Search_BooleanNotQuery_ExcludesDocuments()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act — все python кроме junior
        var response = engine.Search("python AND NOT junior", useSimpleMode: false);

        // Assert
        response.Results.Should().NotBeEmpty();
        response.Results.Should().NotContain(r => r.Vacancy.DocId == 3); // Junior исключён
        response.Results.Should().Contain(r => r.Vacancy.DocId == 1); // Senior остался
    }

    [Fact]
    public void Search_SimpleModeWithOperators_SwitchesToBoolean()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act — явный AND переключает в булев режим
        var response = engine.Search("python AND senior", useSimpleMode: true);

        // Assert
        response.Results.Should().HaveCount(1);
        response.Results[0].Vacancy.DocId.Should().Be(1);
    }

    [Fact]
    public void Search_ResultsRankedByRelevance()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python django", useSimpleMode: true);

        // Assert — документ с обоими словами должен быть выше
        response.Results.Should().NotBeEmpty();
        var topResult = response.Results.First();
        topResult.MatchedTerms.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Search_ReturnsMatchedTerms()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python django", useSimpleMode: true);

        // Assert
        response.Results.Should().Contain(r => r.MatchedTerms.Contains("python"));
        response.Results.Should().Contain(r => r.MatchedTerms.Contains("django"));
    }

    [Fact]
    public void Search_RecordsSearchDuration()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python");

        // Assert
        response.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Search_ReturnsOriginalAndParsedQuery()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("python AND java");

        // Assert
        response.OriginalQuery.Should().Be("python AND java");
        response.ParsedQuery.Should().Contain("python");
        response.ParsedQuery.Should().Contain("java");
        response.ParsedQuery.Should().Contain("AND");
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("");

        // Assert
        response.Results.Should().BeEmpty();
        response.TotalFound.Should().Be(0);
    }

    [Fact]
    public void Search_NonExistentTerm_ReturnsEmptyResults()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("несуществующийтерм12345");

        // Assert
        response.Results.Should().BeEmpty();
    }

    [Fact]
    public void Search_InvalidBooleanQuery_ReturnsError()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var response = engine.Search("(python AND", useSimpleMode: false);

        // Assert
        response.ParsedQuery.Should().Contain("Ошибка");
        response.Results.Should().BeEmpty();
    }

    [Fact]
    public void Search_WithoutLoadedIndex_ThrowsException()
    {
        // Arrange
        var engine = new SearchEngine();

        // Act
        var act = () => engine.Search("python");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*индекс*");
    }

    [Fact]
    public void LookupTerm_ExistingTerm_ReturnsPostingList()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var postingList = engine.LookupTerm("python");

        // Assert
        postingList.Should().NotBeNull();
        postingList!.DocumentFrequency.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LookupTerm_NonExistentTerm_ReturnsNull()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var postingList = engine.LookupTerm("несуществует");

        // Assert
        postingList.Should().BeNull();
    }

    [Fact]
    public void Search_ComplexRealWorldQuery_WorksCorrectly()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act — "(python OR java) AND москва AND NOT junior"
        var response = engine.Search("(python OR java) AND москва AND NOT junior", useSimpleMode: false);

        // Assert
        response.Results.Should().NotBeEmpty();
        response.Results.Should().NotContain(r => r.Vacancy.DocId == 3); // junior исключён
        response.Results.All(r =>
            r.Vacancy.Location.Contains("Москва", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    [Fact]
    public void Statistics_AfterLoadingIndex_Available()
    {
        // Arrange
        var engine = CreateEngineWithTestData();

        // Act
        var stats = engine.Statistics;

        // Assert
        stats.Should().NotBeNull();
        stats!.TotalDocuments.Should().Be(5);
        stats.TotalTerms.Should().BeGreaterThan(0);
        stats.TotalPostings.Should().BeGreaterThan(0);
    }
}