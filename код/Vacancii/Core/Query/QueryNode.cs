using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Query
{
    public enum QueryNodeType
    {
        Term,       // Лист — конкретный терм
        And,        // AND
        Or,         // OR
        Not         // NOT (унарный)
    }

    /// <summary>
    /// Узел дерева разбора булева запроса
    /// </summary>
    public class QueryNode
    {
        public QueryNodeType Type { get; set; }
        public string? Term { get; set; }           // Только для Type == Term
        public QueryNode? Left { get; set; }         // Левый операнд
        public QueryNode? Right { get; set; }        // Правый (или единственный для NOT)

        public static QueryNode CreateTerm(string term)
            => new() { Type = QueryNodeType.Term, Term = term };

        public static QueryNode CreateAnd(QueryNode left, QueryNode right)
            => new() { Type = QueryNodeType.And, Left = left, Right = right };

        public static QueryNode CreateOr(QueryNode left, QueryNode right)
            => new() { Type = QueryNodeType.Or, Left = left, Right = right };

        public static QueryNode CreateNot(QueryNode operand)
            => new() { Type = QueryNodeType.Not, Right = operand };

        public override string ToString()
        {
            return Type switch
            {
                QueryNodeType.Term => Term ?? "",
                QueryNodeType.And => $"({Left} AND {Right})",
                QueryNodeType.Or => $"({Left} OR {Right})",
                QueryNodeType.Not => $"(NOT {Right})",
                _ => "?"
            };
        }
    }
}
