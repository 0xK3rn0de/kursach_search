using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Vacancy
    {
        public int DocId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string Employment { get; set; } = string.Empty; // Full-time, Part-time, Remote
        public List<string> Tags { get; set; } = new();
        public DateTime DatePosted { get; set; }

        /// <summary>
        /// Собирает весь текст вакансии для индексации
        /// </summary>
        public string GetFullText()
        {
            return $"{Title} {Company} {Description} {Location} {Employment} {string.Join(" ", Tags)}";
        }

        public override string ToString()
        {
            var salary = SalaryFrom.HasValue || SalaryTo.HasValue
                ? $"{SalaryFrom?.ToString("N0") ?? "?"} - {SalaryTo?.ToString("N0") ?? "?"} руб."
                : "не указана";
            return $"[{DocId}] {Title} @ {Company} | {Location} | {salary}";
        }
    }
}
