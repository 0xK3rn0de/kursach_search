using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    /// <summary>
    /// Постинг-лист — отсортированный список документов, содержащих терм
    /// </summary>
    public class PostingList
    {
        public string Term { get; set; } = string.Empty;
        public List<Posting> Postings { get; set; } = new();

        public int DocumentFrequency => Postings.Count;

        /// <summary>
        /// Возвращает отсортированный список DocId
        /// </summary>
        public List<int> GetDocIds()
        {
            return Postings.Select(p => p.DocId).OrderBy(x => x).ToList();
        }

        public void AddPosting(int docId, int position)
        {
            var existing = Postings.FirstOrDefault(p => p.DocId == docId);
            if (existing != null)
            {
                existing.Positions.Add(position);
            }
            else
            {
                Postings.Add(new Posting
                {
                    DocId = docId,
                    Positions = new List<int> { position }
                });
            }
        }
    }
}
