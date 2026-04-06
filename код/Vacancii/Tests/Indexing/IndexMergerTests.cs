using Core.Indexing;
using Core.Models;
using FluentAssertions;

namespace Tests;

public class IndexMergerTests
{
    private readonly IndexMerger _merger = new();

    [Fact]
    public void MergeBlocks_TwoBlocks_MergesCorrectly()
    {
        // Arrange
        var block1 = new SPIMIBlock
        {
            BlockId = 0,
            Index = new Dictionary<string, PostingList>
            {
                ["python"] = CreatePostingList("python", (1, new[] { 0 }), (2, new[] { 0 })),
                ["django"] = CreatePostingList("django", (1, new[] { 1 }))
            }
        };

        var block2 = new SPIMIBlock
        {
            BlockId = 1,
            Index = new Dictionary<string, PostingList>
            {
                ["python"] = CreatePostingList("python", (3, new[] { 0 })),
                ["flask"] = CreatePostingList("flask", (3, new[] { 1 }))
            }
        };

        // Act
        var merged = _merger.MergeBlocks(new List<SPIMIBlock> { block1, block2 });

        // Assert
        merged.Should().HaveCount(3); // python, django, flask

        merged["python"].DocumentFrequency.Should().Be(3); // doc 1, 2, 3
        merged["django"].DocumentFrequency.Should().Be(1);
        merged["flask"].DocumentFrequency.Should().Be(1);
    }

    [Fact]
    public void MergeBlocks_SingleBlock_ReturnsSortedIndex()
    {
        // Arrange
        var block = new SPIMIBlock
        {
            BlockId = 0,
            Index = new Dictionary<string, PostingList>
            {
                ["zebra"] = CreatePostingList("zebra", (1, new[] { 0 })),
                ["alpha"] = CreatePostingList("alpha", (1, new[] { 1 })),
                ["middle"] = CreatePostingList("middle", (1, new[] { 2 }))
            }
        };

        // Act
        var merged = _merger.MergeBlocks(new List<SPIMIBlock> { block });

        // Assert
        merged.Keys.Should().BeInAscendingOrder(); // alpha, middle, zebra
    }

    [Fact]
    public void MergeBlocks_EmptyList_ReturnsEmptyIndex()
    {
        // Act
        var merged = _merger.MergeBlocks(new List<SPIMIBlock>());

        // Assert
        merged.Should().BeEmpty();
    }

    [Fact]
    public void MergeBlocks_OverlappingPostings_MergesPositions()
    {
        // Arrange — один и тот же doc в разных блоках (теоретически не должно быть, но тест на устойчивость)
        var block1 = new SPIMIBlock
        {
            BlockId = 0,
            Index = new Dictionary<string, PostingList>
            {
                ["term"] = CreatePostingList("term", (1, new[] { 0, 1 }))
            }
        };

        var block2 = new SPIMIBlock
        {
            BlockId = 1,
            Index = new Dictionary<string, PostingList>
            {
                ["term"] = CreatePostingList("term", (1, new[] { 5, 6 }))
            }
        };

        // Act
        var merged = _merger.MergeBlocks(new List<SPIMIBlock> { block1, block2 });

        // Assert
        var posting = merged["term"].Postings.First(p => p.DocId == 1);
        posting.Positions.Should().Contain(new[] { 0, 1, 5, 6 });
    }

    [Fact]
    public void MergeBlocks_ResultIsSortedByDocId()
    {
        // Arrange
        var block1 = new SPIMIBlock
        {
            BlockId = 0,
            Index = new Dictionary<string, PostingList>
            {
                ["term"] = CreatePostingList("term", (5, new[] { 0 }), (2, new[] { 0 }))
            }
        };

        var block2 = new SPIMIBlock
        {
            BlockId = 1,
            Index = new Dictionary<string, PostingList>
            {
                ["term"] = CreatePostingList("term", (8, new[] { 0 }), (1, new[] { 0 }))
            }
        };

        // Act
        var merged = _merger.MergeBlocks(new List<SPIMIBlock> { block1, block2 });

        // Assert
        var docIds = merged["term"].GetDocIds();
        docIds.Should().BeInAscendingOrder();
        docIds.Should().ContainInOrder(1, 2, 5, 8);
    }

    private static PostingList CreatePostingList(string term, params (int docId, int[] positions)[] postings)
    {
        var pl = new PostingList { Term = term };
        foreach (var (docId, positions) in postings)
        {
            pl.Postings.Add(new Posting { DocId = docId, Positions = positions.ToList() });
        }
        return pl;
    }
}
