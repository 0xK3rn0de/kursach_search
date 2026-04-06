using Core.Models;
using Core.Query;
using FluentAssertions;

namespace Tests;

public class BooleanQueryEvaluatorTests
{
    private static InvertedIndex CreateTestIndex()
    {
        var index = new InvertedIndex
        {
            Documents = new Dictionary<int, Vacancy>
            {
                [1] = new() { DocId = 1, Title = "Python Django", Company = "", Description = "", Location = "", Employment = "" },
                [2] = new() { DocId = 2, Title = "Java Spring", Company = "", Description = "", Location = "", Employment = "" },
                [3] = new() { DocId = 3, Title = "Python Flask", Company = "", Description = "", Location = "", Employment = "" },
                [4] = new() { DocId = 4, Title = "C# .NET", Company = "", Description = "", Location = "", Employment = "" },
                [5] = new() { DocId = 5, Title = "Python Java", Company = "", Description = "", Location = "", Employment = "" }
            }
        };

        index.Index = new SortedDictionary<string, PostingList>
        {
            ["python"] = CreatePostingList("python", 1, 3, 5),
            ["django"] = CreatePostingList("django", 1),
            ["java"] = CreatePostingList("java", 2, 5),
            ["spring"] = CreatePostingList("spring", 2),
            ["flask"] = CreatePostingList("flask", 3),
            ["c#"] = CreatePostingList("c#", 4),
            [".net"] = CreatePostingList(".net", 4)
        };

        return index;
    }

    private static PostingList CreatePostingList(string term, params int[] docIds)
    {
        var pl = new PostingList { Term = term };
        foreach (var docId in docIds)
        {
            pl.Postings.Add(new Posting { DocId = docId, Positions = new List<int> { 0 } });
        }
        return pl;
    }

    [Fact]
    public void Evaluate_SingleTerm_ReturnsMatchingDocs()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        var query = QueryNode.CreateTerm("python");

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }

    [Fact]
    public void Evaluate_TermNotInIndex_ReturnsEmpty()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        var query = QueryNode.CreateTerm("rust");

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_And_ReturnsIntersection()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // python: [1, 3, 5], java: [2, 5] → intersection: [5]
        var query = QueryNode.CreateAnd(
            QueryNode.CreateTerm("python"),
            QueryNode.CreateTerm("java")
        );

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 5 });
    }

    [Fact]
    public void Evaluate_Or_ReturnsUnion()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // python: [1, 3, 5], java: [2, 5] → union: [1, 2, 3, 5]
        var query = QueryNode.CreateOr(
            QueryNode.CreateTerm("python"),
            QueryNode.CreateTerm("java")
        );

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 1, 2, 3, 5 });
    }

    [Fact]
    public void Evaluate_Not_ReturnsComplement()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // python: [1, 3, 5], all: [1, 2, 3, 4, 5] → NOT python: [2, 4]
        var query = QueryNode.CreateNot(QueryNode.CreateTerm("python"));

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 2, 4 });
    }

    [Fact]
    public void Evaluate_ComplexQuery_WorksCorrectly()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // (python OR java) AND NOT flask
        // python OR java: [1, 2, 3, 5]
        // flask: [3]
        // result: [1, 2, 5]
        var query = QueryNode.CreateAnd(
            QueryNode.CreateOr(
                QueryNode.CreateTerm("python"),
                QueryNode.CreateTerm("java")
            ),
            QueryNode.CreateNot(QueryNode.CreateTerm("flask"))
        );

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 1, 2, 5 });
    }

    [Fact]
    public void Evaluate_NullQuery_ReturnsEmpty()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);

        // Act
        var result = evaluator.Evaluate(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_AndWithEmptyResult_ReturnsEmpty()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // django: [1], spring: [2] → intersection: []
        var query = QueryNode.CreateAnd(
            QueryNode.CreateTerm("django"),
            QueryNode.CreateTerm("spring")
        );

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_ResultsSorted()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        var query = QueryNode.CreateTerm("python");

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeInAscendingOrder();
    }

    [Fact]
    public void Evaluate_ChainedAnd_WorksCorrectly()
    {
        // Arrange
        var index = CreateTestIndex();
        var evaluator = new BooleanQueryEvaluator(index);
        // python AND django
        // python: [1, 3, 5], django: [1] → [1]
        var query = QueryNode.CreateAnd(
            QueryNode.CreateTerm("python"),
            QueryNode.CreateTerm("django")
        );

        // Act
        var result = evaluator.Evaluate(query);

        // Assert
        result.Should().BeEquivalentTo(new[] { 1 });
    }
}
