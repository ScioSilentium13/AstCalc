namespace AstCalculator.Core.Types
{
    /// <summary>
    /// Операция с одним операндом
    /// </summary>
    public class UnaryOperation : Expression
    {
        public Expression Expression { get; }
        public UnaryOperationType OperationType { get; }

        public UnaryOperation(Expression expression, UnaryOperationType operationType)
        {
            Expression = expression;
            OperationType = operationType;
        }

        public override double Evaluate()
        {
            var eval = Expression.Evaluate();

            var result = OperationType switch
            { 
                UnaryOperationType.Negate => eval * -1,
                _ => throw new Exception("Unknown unary operation")
            };

            return result;
        }

        public override string ToString()
        {
            return OperationType switch
            {
                UnaryOperationType.Negate => $"-({Expression})",
                _ => throw new Exception("Unknown unary operation")
            };
        }
    }
}
