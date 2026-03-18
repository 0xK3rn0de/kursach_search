using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    /// <summary>
    /// Блок SPIMI — словарь термов с их постинг-листами,
    /// формируемый в памяти до достижения лимита
    /// </summary>
    public class SPIMIBlock
    {
        public int BlockId { get; set; }
        public Dictionary<string, PostingList> Index { get; set; } = new();
        public long ApproximateSizeBytes { get; set; }

        /// <summary>
        /// Возвращает отсортированный индекс блока
        /// </summary>
        public SortedDictionary<string, PostingList> GetSortedIndex()
        {
            return new SortedDictionary<string, PostingList>(Index);
        }
    }
}
