using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    /// <summary>
    /// Одна запись в постинг-листе: ссылка на документ + позиции терма
    /// </summary>
    public class Posting : IComparable<Posting>
    {
        public int DocId { get; set; }
        public List<int> Positions { get; set; } = new();

        public int CompareTo(Posting? other)
        {
            if (other == null) return 1;
            return DocId.CompareTo(other.DocId);
        }
    }
}
