using Core.Query;
using FluentAssertions;

namespace Tests;

public class BooleanQueryParserTests
{
    private readonly BooleanQueryParser _parser = new();

    [Fact]
    public void Parse_SingleTerm_ReturnsTermNode()
    {
        // Act
        var result = _parser.Parse("python");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Term);
        result.Term.Should().Be("python");
    }

    [Fact]
    public void Parse_AndExpression_ReturnsAndNode()
    {
        // Act
        var result = _parser.Parse("python AND django");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.And);
        result.Left!.Type.Should().Be(QueryNodeType.Term);
        result.Left.Term.Should().Be("python");
        result.Right!.Type.Should().Be(QueryNodeType.Term);
        result.Right.Term.Should().Be("django");
    }

    [Fact]
    public void Parse_OrExpression_ReturnsOrNode()
    {
        // Act
        var result = _parser.Parse("python OR java");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Or);
        result.Left!.Term.Should().Be("python");
        result.Right!.Term.Should().Be("java");
    }

    [Fact]
    public void Parse_NotExpression_ReturnsNotNode()
    {
        // Act
        var result = _parser.Parse("NOT junior");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Not);
        result.Right!.Type.Should().Be(QueryNodeType.Term);
        result.Right.Term.Should().Be("junior");
    }

    [Fact]
    public void Parse_ComplexExpression_RespectsParentheses()
    {
        // Act
        var result = _parser.Parse("(python OR java) AND senior");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.And);
        result.Left!.Type.Should().Be(QueryNodeType.Or);
        result.Right!.Type.Should().Be(QueryNodeType.Term);
        result.Right.Term.Should().Be("senior");
    }

    [Fact]
    public void Parse_OperatorPrecedence_AndBeforeOr()
    {
        // "a OR b AND c" должно парситься как "a OR (b AND c)"
        // Act
        var result = _parser.Parse("python OR java AND spring");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Or);
        result.Left!.Term.Should().Be("python");
        result.Right!.Type.Should().Be(QueryNodeType.And);
    }

    [Fact]
    public void Parse_ChainedAnd_LeftAssociative()
    {
        // "a AND b AND c" → ((a AND b) AND c)
        // Act
        var result = _parser.Parse("a AND b AND c");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.And);
        result.Left!.Type.Should().Be(QueryNodeType.And);
        result.Right!.Term.Should().Be("c");
    }

    [Fact]
    public void Parse_DoubleNot_Allowed()
    {
        // Act
        var result = _parser.Parse("NOT NOT term");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Not);
        result.Right!.Type.Should().Be(QueryNodeType.Not);
        result.Right.Right!.Term.Should().Be("term");
    }

    [Fact]
    public void Parse_CaseInsensitiveOperators_Recognized()
    {
        // Act
        var result = _parser.Parse("python and django or flask");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Or);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        // Act
        var result = _parser.Parse("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsNull()
    {
        // Act
        var result = _parser.Parse("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_UnmatchedOpenParen_ThrowsException()
    {
        // Act
        var act = () => _parser.Parse("(python AND django");

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_UnmatchedCloseParen_ThrowsException()
    {
        // Act
        var act = () => _parser.Parse("python AND django)");

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseSimple_WithOperators_SwitchesToFullParser()
    {
        // Act
        var result = _parser.ParseSimple("python AND django");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.And);
    }

    [Fact]
    public void ParseSimple_WithParentheses_SwitchesToFullParser()
    {
        // Act
        var result = _parser.ParseSimple("(python OR java)");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.Or);
    }

    [Fact]
    public void Parse_RealWorldQuery_ParsesCorrectly()
    {
        // Act
        var result = _parser.Parse("(python OR java) AND senior AND NOT стажировка");

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(QueryNodeType.And);
        result.ToString().Should().Contain("python");
        result.ToString().Should().Contain("java");
        result.ToString().Should().Contain("senior");
        result.ToString().Should().Contain("NOT");
    }
}
