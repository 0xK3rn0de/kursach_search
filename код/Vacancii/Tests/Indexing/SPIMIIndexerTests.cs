using Core.Indexing;
using Core.Models;
using FluentAssertions;

namespace Tests;

public class SPIMIIndexerTests
{
    [Fact]
    public void BuildIndex_SingleDocument_CreatesCorrectIndex()
    {
        // Arrange
        var indexer = new SPIMIIndexer(blockSizeLimit: 1000);
        var vacancies = new List<Vacancy>
            {
                new()
                {
                    DocId = 1,
                    Title = "Python разработчик",
                    Company = "Яндекс",
                    Description = "Ищем Python разработчика",
                    Location = "Москва",
                    Employment = "Полная занятость",
                    Tags = new List<string> { "python", "django" }
                }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        index.TotalDocuments.Should().Be(1);
        index.TotalTerms.Should().BeGreaterThan(0);
        index.GetPostingList("python").Should().NotBeNull();
        index.GetPostingList("python")!.DocumentFrequency.Should().Be(1);
    }

    [Fact]
    public void BuildIndex_MultipleDocuments_IndexesAllDocuments()
    {
        // Arrange
        var indexer = new SPIMIIndexer(blockSizeLimit: 1000);
        var vacancies = new List<Vacancy>
            {
                new() { DocId = 1, Title = "Python разработчик", Description = "Django Flask", Company = "", Location = "", Employment = "" },
                new() { DocId = 2, Title = "Java разработчик", Description = "Spring Boot", Company = "", Location = "", Employment = "" },
                new() { DocId = 3, Title = "Python и Java", Description = "Fullstack", Company = "", Location = "", Employment = "" }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        index.TotalDocuments.Should().Be(3);

        var pythonPostings = index.GetPostingList("python");
        pythonPostings.Should().NotBeNull();
        pythonPostings!.DocumentFrequency.Should().Be(2); // doc 1 и 3

        var javaPostings = index.GetPostingList("java");
        javaPostings.Should().NotBeNull();
        javaPostings!.DocumentFrequency.Should().Be(2); // doc 2 и 3
    }

    [Fact]
    public void BuildIndex_SmallBlockSize_CreatesMultipleBlocks()
    {
        // Arrange
        var indexer = new SPIMIIndexer(blockSizeLimit: 5); // очень маленький блок
        var blocksFlushed = new List<int>();
        indexer.OnBlockFlushed += blockId => blocksFlushed.Add(blockId);

        var vacancies = new List<Vacancy>
            {
                new() { DocId = 1, Title = "Python Django Flask PostgreSQL Redis", Description = "Docker Kubernetes", Company = "", Location = "", Employment = "" },
                new() { DocId = 2, Title = "Java Spring Hibernate Maven Kafka", Description = "Microservices", Company = "", Location = "", Employment = "" }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        blocksFlushed.Should().HaveCountGreaterThan(1); // несколько блоков создано
        index.TotalDocuments.Should().Be(2);
    }

    [Fact]
    public void BuildIndex_PositionalIndex_StoresPositions()
    {
        // Arrange
        var indexer = new SPIMIIndexer(blockSizeLimit: 1000);
        var vacancies = new List<Vacancy>
            {
                new()
                {
                    DocId = 1,
                    Title = "Python",
                    Description = "Python Python Python", // python встречается 4 раза
                    Company = "",
                    Location = "",
                    Employment = ""
                }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        var postingList = index.GetPostingList("python");
        postingList.Should().NotBeNull();
        var posting = postingList!.Postings.First(p => p.DocId == 1);
        posting.Positions.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void BuildIndex_EmptyCollection_ReturnsEmptyIndex()
    {
        // Arrange
        var indexer = new SPIMIIndexer();
        var vacancies = new List<Vacancy>();

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        index.TotalDocuments.Should().Be(0);
        index.TotalTerms.Should().Be(0);
    }

    [Fact]
    public void BuildIndex_PostingListsSortedByDocId()
    {
        // Arrange
        var indexer = new SPIMIIndexer();
        var vacancies = new List<Vacancy>
            {
                new() { DocId = 5, Title = "Python", Description = "", Company = "", Location = "", Employment = "" },
                new() { DocId = 2, Title = "Python", Description = "", Company = "", Location = "", Employment = "" },
                new() { DocId = 8, Title = "Python", Description = "", Company = "", Location = "", Employment = "" },
                new() { DocId = 1, Title = "Python", Description = "", Company = "", Location = "", Employment = "" }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        var postingList = index.GetPostingList("python");
        var docIds = postingList!.GetDocIds();
        docIds.Should().BeInAscendingOrder();
    }

    [Fact]
    public void BuildIndex_RecordsBuildStatistics()
    {
        // Arrange
        var indexer = new SPIMIIndexer();
        var vacancies = new List<Vacancy>
            {
                new() { DocId = 1, Title = "Python", Description = "Django", Company = "", Location = "", Employment = "" }
            };

        // Act
        var index = indexer.BuildIndex(vacancies);

        // Assert
        index.BuildDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        index.BuildDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
