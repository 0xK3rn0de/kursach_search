using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Query
{
    /// <summary>
    /// Вычислитель булевых запросов на инвертированном индексе.
    /// Все операции работают на отсортированных списках docId.
    /// </summary>
    public class BooleanQueryEvaluator
    {
        private readonly InvertedIndex _index;

        public BooleanQueryEvaluator(InvertedIndex index)
        {
            _index = index;
        }

        /// <summary>
        /// Вычислить запрос, вернуть список подходящих docId
        /// </summary>
        public List<int> Evaluate(QueryNode? node)
        {
            if (node == null)
                return new List<int>();

            return node.Type switch
            {
                QueryNodeType.Term => EvaluateTerm(node.Term!),
                QueryNodeType.And => Intersect(Evaluate(node.Left), Evaluate(node.Right)),
                QueryNodeType.Or => Union(Evaluate(node.Left), Evaluate(node.Right)),
                QueryNodeType.Not => Complement(Evaluate(node.Right)),
                _ => new List<int>()
            };
        }

        private List<int> EvaluateTerm(string term)
        {
            var postingList = _index.GetPostingList(term);
            return postingList?.GetDocIds() ?? new List<int>();
        }

        /// <summary>
        /// Пересечение двух отсортированных списков (AND)
        /// O(n + m) — линейное слияние
        /// </summary>
        private List<int> Intersect(List<int> a, List<int> b)
        {
            var result = new List<int>();
            int i = 0, j = 0;

            while (i < a.Count && j < b.Count)
            {
                if (a[i] == b[j])
                {
                    result.Add(a[i]);
                    i++;
                    j++;
                }
                else if (a[i] < b[j])
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            return result;
        }

        /// <summary>
        /// Объединение двух отсортированных списков (OR)
        /// O(n + m)
        /// </summary>
        private List<int> Union(List<int> a, List<int> b)
        {
            var result = new List<int>();
            int i = 0, j = 0;

            while (i < a.Count && j < b.Count)
            {
                if (a[i] == b[j])
                {
                    result.Add(a[i]);
                    i++;
                    j++;
                }
                else if (a[i] < b[j])
                {
                    result.Add(a[i]);
                    i++;
                }
                else
                {
                    result.Add(b[j]);
                    j++;
                }
            }

            while (i < a.Count) result.Add(a[i++]);
            while (j < b.Count) result.Add(b[j++]);

            return result;
        }

        /// <summary>
        /// Дополнение (NOT) — все документы минус указанные
        /// </summary>
        private List<int> Complement(List<int> docIds)
        {
            var allDocs = _index.Documents.Keys.OrderBy(x => x).ToList();
            var excludeSet = new HashSet<int>(docIds);

            return allDocs.Where(id => !excludeSet.Contains(id)).ToList();
        }
    }
}
