using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Indexing
{
    /// <summary>
    /// Слияние SPIMI-блоков в единый индекс (multi-way merge)
    /// </summary>
    public class IndexMerger
    {
        /// <summary>
        /// Слияние нескольких блоков.
        /// Используем k-way merge: для каждого терма объединяем постинг-листы из всех блоков
        /// </summary>
        public SortedDictionary<string, PostingList> MergeBlocks(List<SPIMIBlock> blocks)
        {
            if (blocks.Count == 0)
                return new SortedDictionary<string, PostingList>();

            if (blocks.Count == 1)
                return blocks[0].GetSortedIndex();

            // Получаем отсортированные индексы из каждого блока
            var sortedBlocks = blocks.Select(b => b.GetSortedIndex()).ToList();

            var mergedIndex = new SortedDictionary<string, PostingList>();

            // Собираем все уникальные термы из всех блоков
            var allTerms = new SortedSet<string>();
            foreach (var block in sortedBlocks)
            {
                foreach (var term in block.Keys)
                {
                    allTerms.Add(term);
                }
            }

            // Для каждого терма — мержим постинг-листы
            foreach (var term in allTerms)
            {
                var mergedPostingList = new PostingList { Term = term };
                var allPostings = new Dictionary<int, Posting>(); // docId → Posting

                foreach (var block in sortedBlocks)
                {
                    if (block.TryGetValue(term, out var postingList))
                    {
                        foreach (var posting in postingList.Postings)
                        {
                            if (allPostings.TryGetValue(posting.DocId, out var existing))
                            {
                                // Мержим позиции
                                existing.Positions.AddRange(posting.Positions);
                            }
                            else
                            {
                                allPostings[posting.DocId] = new Posting
                                {
                                    DocId = posting.DocId,
                                    Positions = new List<int>(posting.Positions)
                                };
                            }
                        }
                    }
                }

                // Сортируем постинги по docId
                mergedPostingList.Postings = allPostings.Values
                    .OrderBy(p => p.DocId)
                    .ToList();

                // Сортируем позиции внутри каждого постинга
                foreach (var p in mergedPostingList.Postings)
                {
                    p.Positions.Sort();
                }

                mergedIndex[term] = mergedPostingList;
            }

            return mergedIndex;
        }
    }
}
