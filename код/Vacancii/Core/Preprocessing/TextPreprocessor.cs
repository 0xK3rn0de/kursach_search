using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Preprocessing
{
    /// <summary>
    /// Фасад для всего пайплайна предобработки текста
    /// </summary>
    public class TextPreprocessor
    {
        private readonly Tokenizer _tokenizer;
        private readonly TextNormalizer _normalizer;
        private readonly StopWordsFilter _stopWordsFilter;
        private readonly bool _removeStopWords;

        public TextPreprocessor(bool removeStopWords = true)
        {
            _tokenizer = new Tokenizer();
            _normalizer = new TextNormalizer();
            _stopWordsFilter = new StopWordsFilter();
            _removeStopWords = removeStopWords;
        }

        /// <summary>
        /// Полный пайплайн: текст → токены → нормализация → фильтрация
        /// </summary>
        public List<string> Process(string text)
        {
            var tokens = _tokenizer.Tokenize(text);
            tokens = _normalizer.NormalizeAll(tokens);

            if (_removeStopWords)
                tokens = _stopWordsFilter.Filter(tokens);

            return tokens;
        }

        /// <summary>
        /// Обработка с сохранением позиций (для позиционного индекса)
        /// </summary>
        public List<(string Term, int Position)> ProcessWithPositions(string text)
        {
            var tokensWithPos = _tokenizer.TokenizeWithPositions(text);
            var result = new List<(string, int)>();

            foreach (var (token, pos) in tokensWithPos)
            {
                var normalized = _normalizer.Normalize(token);
                if (normalized.Length > 0 && (!_removeStopWords || !_stopWordsFilter.IsStopWord(normalized)))
                {
                    result.Add((normalized, pos));
                }
            }

            return result;
        }
    }
}
