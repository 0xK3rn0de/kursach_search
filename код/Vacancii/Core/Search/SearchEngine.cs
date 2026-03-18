using Core.Models;
using Core.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Search
{
    public class SearchResult
    {
        public Vacancy Vacancy { get; set; } = null!;
        public double Score { get; set; }
        public List<string> MatchedTerms { get; set; } = new();
    }

    public class SearchResponse
    {
        public string OriginalQuery { get; set; } = string.Empty;
        public string ParsedQuery { get; set; } = string.Empty;
        public List<SearchResult> Results { get; set; } = new();
        public int TotalFound { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }

    public class SearchEngine
    {
        private InvertedIndex? _index;
        private readonly BooleanQueryParser _parser;

        public bool IsIndexLoaded => _index != null;
        public IndexStatistics? Statistics => _index?.GetStatistics();

        public SearchEngine()
        {
            _parser = new BooleanQueryParser();
        }

        public void LoadIndex(InvertedIndex index)
        {
            _index = index;
        }

        /// <summary>
        /// Выполнить поисковый запрос
        /// </summary>
        public SearchResponse Search(string query, bool useSimpleMode = true)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (_index == null)
                throw new InvalidOperationException("Индекс не загружен");

            // Парсим запрос
            QueryNode? queryTree;
            try
            {
                queryTree = useSimpleMode
                    ? _parser.ParseSimple(query)
                    : _parser.Parse(query);
            }
            catch (FormatException ex)
            {
                return new SearchResponse
                {
                    OriginalQuery = query,
                    ParsedQuery = $"Ошибка: {ex.Message}",
                    Results = new(),
                    SearchDuration = stopwatch.Elapsed
                };
            }

            if (queryTree == null)
            {
                return new SearchResponse
                {
                    OriginalQuery = query,
                    ParsedQuery = "(пустой запрос)",
                    Results = new(),
                    SearchDuration = stopwatch.Elapsed
                };
            }

            // Вычисляем
            var evaluator = new BooleanQueryEvaluator(_index);
            var docIds = evaluator.Evaluate(queryTree);

            // Собираем результаты с подсчётом релевантности
            var results = new List<SearchResult>();
            var queryTerms = ExtractTerms(queryTree);

            foreach (var docId in docIds)
            {
                if (_index.Documents.TryGetValue(docId, out var vacancy))
                {
                    var matchedTerms = new List<string>();
                    double score = 0;

                    foreach (var term in queryTerms)
                    {
                        var pl = _index.GetPostingList(term);
                        if (pl != null)
                        {
                            var posting = pl.Postings.FirstOrDefault(p => p.DocId == docId);
                            if (posting != null)
                            {
                                matchedTerms.Add(term);
                                // Простой TF-based скоринг
                                score += posting.Positions.Count *
                                         Math.Log10((double)_index.TotalDocuments / pl.DocumentFrequency);
                            }
                        }
                    }

                    results.Add(new SearchResult
                    {
                        Vacancy = vacancy,
                        Score = score,
                        MatchedTerms = matchedTerms
                    });
                }
            }

            // Сортируем по релевантности
            results = results.OrderByDescending(r => r.Score).ToList();

            stopwatch.Stop();

            return new SearchResponse
            {
                OriginalQuery = query,
                ParsedQuery = queryTree.ToString(),
                Results = results,
                TotalFound = results.Count,
                SearchDuration = stopwatch.Elapsed
            };
        }

        /// <summary>
        /// Получить информацию о терме в индексе
        /// </summary>
        public PostingList? LookupTerm(string term)
        {
            return _index?.GetPostingList(term.ToLowerInvariant());
        }

        private List<string> ExtractTerms(QueryNode node)
        {
            var terms = new List<string>();

            if (node.Type == QueryNodeType.Term && node.Term != null)
            {
                terms.Add(node.Term);
            }

            if (node.Left != null) terms.AddRange(ExtractTerms(node.Left));
            if (node.Right != null) terms.AddRange(ExtractTerms(node.Right));

            return terms.Distinct().ToList();
        }
    }
}
