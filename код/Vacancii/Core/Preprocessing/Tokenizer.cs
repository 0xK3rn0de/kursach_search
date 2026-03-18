using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Preprocessing
{
    public class Tokenizer
    {
        private static readonly Regex TokenPattern = new(
            @"[a-zA-Zа-яА-ЯёЁ0-9#\+\.]+",
            RegexOptions.Compiled);

        /// <summary>
        /// Разбивает текст на токены (слова)
        /// </summary>
        public List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var matches = TokenPattern.Matches(text);
            var tokens = new List<string>(matches.Count);

            foreach (Match match in matches)
            {
                tokens.Add(match.Value);
            }

            return tokens;
        }

        /// <summary>
        /// Разбивает текст на токены с сохранением позиций
        /// </summary>
        public List<(string Token, int Position)> TokenizeWithPositions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new();

            var matches = TokenPattern.Matches(text);
            var result = new List<(string, int)>(matches.Count);
            int position = 0;

            foreach (Match match in matches)
            {
                result.Add((match.Value, position));
                position++;
            }

            return result;
        }
    }
}
