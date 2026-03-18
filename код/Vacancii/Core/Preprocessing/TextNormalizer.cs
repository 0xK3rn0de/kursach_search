using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Preprocessing
{
    public class TextNormalizer
    {
        /// <summary>
        /// Приводит токен к нормальной форме (lowercase).
        /// Для полноценного стемминга можно подключить библиотеку.
        /// </summary>
        public string Normalize(string token)
        {
            return token.ToLowerInvariant().Trim('.');
        }

        public List<string> NormalizeAll(List<string> tokens)
        {
            return tokens.Select(Normalize).Where(t => t.Length > 0).ToList();
        }
    }
}
