using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    /// <summary>
    /// Полный инвертированный индекс
    /// </summary>
    public class InvertedIndex
    {
        public SortedDictionary<string, PostingList> Index { get; set; } = new();
        public Dictionary<int, Vacancy> Documents { get; set; } = new();
        public int TotalDocuments => Documents.Count;
        public int TotalTerms => Index.Count;
        public DateTime BuildDate { get; set; }
        public TimeSpan BuildDuration { get; set; }

        public PostingList? GetPostingList(string term)
        {
            Index.TryGetValue(term.ToLowerInvariant(), out var result);
            return result;
        }

        public IndexStatistics GetStatistics()
        {
            return new IndexStatistics
            {
                TotalDocuments = TotalDocuments,
                TotalTerms = TotalTerms,
                TotalPostings = Index.Values.Sum(p => p.DocumentFrequency),
                AveragePostingListLength = Index.Count > 0
                    ? Index.Values.Average(p => p.DocumentFrequency) : 0,
                BuildDate = BuildDate,
                BuildDuration = BuildDuration
            };
        }
    }

    public class IndexStatistics
    {
        public int TotalDocuments { get; set; }
        public int TotalTerms { get; set; }
        public int TotalPostings { get; set; }
        public double AveragePostingListLength { get; set; }
        public DateTime BuildDate { get; set; }
        public TimeSpan BuildDuration { get; set; }
    }
}
