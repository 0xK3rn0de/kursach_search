using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Preprocessing
{
    public class StopWordsFilter
    {
        private readonly HashSet<string> _stopWords;

        public StopWordsFilter()
        {
            _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Русские стоп-слова
                "и", "в", "во", "не", "что", "он", "на", "я", "с", "со",
                "как", "а", "то", "все", "она", "так", "его", "но", "да",
                "ты", "к", "у", "же", "вы", "за", "бы", "по", "только",
                "её", "мне", "было", "вот", "от", "меня", "ещё", "нет",
                "о", "из", "ему", "теперь", "когда", "даже", "ну", "вдруг",
                "ли", "если", "уже", "или", "ни", "быть", "был", "него",
                "до", "вас", "нибудь", "опять", "уж", "вам", "ведь",
                "там", "потом", "себя", "ничего", "ей", "может", "они",
                "тут", "где", "есть", "надо", "ней", "для", "мы", "тебя",
                "их", "чем", "была", "сам", "чтоб", "без", "будто",
                "чего", "раз", "тоже", "себе", "под", "будет", "ж",
                "тогда", "кто", "этот", "того", "потому", "этого",
                "какой", "ним", "при", "этом",

                // Английские стоп-слова
                "the", "a", "an", "is", "are", "was", "were", "be", "been",
                "being", "have", "has", "had", "do", "does", "did", "will",
                "would", "could", "should", "may", "might", "can", "shall",
                "to", "of", "in", "for", "on", "with", "at", "by", "from",
                "as", "into", "through", "during", "before", "after",
                "above", "below", "between", "out", "off", "over", "under",
                "again", "further", "then", "once", "here", "there", "when",
                "where", "why", "how", "all", "both", "each", "few", "more",
                "most", "other", "some", "such", "no", "nor", "not", "only",
                "own", "same", "so", "than", "too", "very", "just", "or",
                "and", "but", "if", "it", "its", "this", "that", "which"
            };
        }

        public bool IsStopWord(string word)
        {
            return _stopWords.Contains(word) || word.Length <= 1;
        }

        public List<string> Filter(List<string> tokens)
        {
            return tokens.Where(t => !IsStopWord(t)).ToList();
        }
    }
}
