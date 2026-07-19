namespace AstCalculator.Core.Types
{
    /// <summary>
    /// Выражение, заключенное в скобки
    /// </summary>
    public class ParenthesizedExpression : Expression
    {
        public Expression Expression { get; set; }

        public ParenthesizedExpression(Expression expression)
        {
            Expression = expression;
        }

        public override double Evaluate()
        {
            return Expression.Evaluate();
        }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }
}
