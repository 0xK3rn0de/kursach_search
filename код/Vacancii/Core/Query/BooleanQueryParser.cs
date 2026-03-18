using Core.Preprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Query
{
    /// <summary>
    /// Парсер булевых запросов.
    /// 
    /// Грамматика:
    ///   query      → orExpr
    ///   orExpr     → andExpr (OR andExpr)*
    ///   andExpr    → notExpr (AND notExpr)*
    ///   notExpr    → NOT notExpr | primary
    ///   primary    → TERM | '(' query ')'
    /// 
    /// Примеры:
    ///   "python AND (django OR flask)"
    ///   "java AND NOT junior"
    ///   "c# AND senior AND NOT remote"
    ///   "москва AND (python OR java) AND NOT стажёр"
    /// </summary>
    public class BooleanQueryParser
    {
        private List<string> _tokens = new();
        private int _pos;
        private readonly TextNormalizer _normalizer = new();

        public QueryNode? Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            _tokens = Tokenize(query);
            _pos = 0;

            if (_tokens.Count == 0)
                return null;

            var result = ParseOrExpression();

            // Если есть неразобранные токены — ошибка
            if (_pos < _tokens.Count)
            {
                throw new FormatException(
                    $"Неожиданный токен '{_tokens[_pos]}' на позиции {_pos}");
            }

            return result;
        }

        /// <summary>
        /// Простой парсинг: слова без операторов объединяются через AND
        /// "python разработчик москва" → python AND разработчик AND москва
        /// </summary>
        public QueryNode? ParseSimple(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            // Проверяем, есть ли операторы — если да, используем полный парсер
            var upper = query.ToUpperInvariant();
            if (upper.Contains("AND") || upper.Contains("OR") || upper.Contains("NOT") ||
                query.Contains("(") || query.Contains(")"))
            {
                return Parse(query);
            }

            // Иначе — просто AND между всеми словами
            var preprocessor = new TextPreprocessor(removeStopWords: false);
            var terms = preprocessor.Process(query);

            if (terms.Count == 0)
                return null;

            var node = QueryNode.CreateTerm(terms[0]);
            for (int i = 1; i < terms.Count; i++)
            {
                node = QueryNode.CreateAnd(node, QueryNode.CreateTerm(terms[i]));
            }

            return node;
        }

        private List<string> Tokenize(string query)
        {
            var tokens = new List<string>();
            int i = 0;

            while (i < query.Length)
            {
                // Пропуск пробелов
                if (char.IsWhiteSpace(query[i]))
                {
                    i++;
                    continue;
                }

                // Скобки
                if (query[i] == '(' || query[i] == ')')
                {
                    tokens.Add(query[i].ToString());
                    i++;
                    continue;
                }

                // Слово или оператор
                int start = i;
                while (i < query.Length && !char.IsWhiteSpace(query[i])
                       && query[i] != '(' && query[i] != ')')
                {
                    i++;
                }

                tokens.Add(query[start..i]);
            }

            return tokens;
        }

        private QueryNode ParseOrExpression()
        {
            var left = ParseAndExpression();

            while (_pos < _tokens.Count && IsOperator("OR"))
            {
                _pos++; // пропускаем "OR"
                var right = ParseAndExpression();
                left = QueryNode.CreateOr(left, right);
            }

            return left;
        }

        private QueryNode ParseAndExpression()
        {
            var left = ParseNotExpression();

            while (_pos < _tokens.Count && IsOperator("AND"))
            {
                _pos++; // пропускаем "AND"
                var right = ParseNotExpression();
                left = QueryNode.CreateAnd(left, right);
            }

            return left;
        }

        private QueryNode ParseNotExpression()
        {
            if (_pos < _tokens.Count && IsOperator("NOT"))
            {
                _pos++; // пропускаем "NOT"
                var operand = ParseNotExpression();
                return QueryNode.CreateNot(operand);
            }

            return ParsePrimary();
        }

        private QueryNode ParsePrimary()
        {
            if (_pos >= _tokens.Count)
                throw new FormatException("Неожиданный конец запроса");

            // Скобки
            if (_tokens[_pos] == "(")
            {
                _pos++; // пропускаем "("
                var node = ParseOrExpression();

                if (_pos >= _tokens.Count || _tokens[_pos] != ")")
                    throw new FormatException("Ожидалась закрывающая скобка ')'");

                _pos++; // пропускаем ")"
                return node;
            }

            // Терм
            var term = _normalizer.Normalize(_tokens[_pos]);
            _pos++;
            return QueryNode.CreateTerm(term);
        }

        private bool IsOperator(string op)
        {
            return _pos < _tokens.Count &&
                   _tokens[_pos].Equals(op, StringComparison.OrdinalIgnoreCase);
        }
    }
}
