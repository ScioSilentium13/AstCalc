using AstCalculator.Core.Exceptions;

namespace AstCalculator.Core.Types
{
    /// <summary>
    /// Операция с двумя операндами
    /// </summary>
    public class BinaryOperation : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public BinaryOperationType OperationType { get; }

        public BinaryOperation(Expression left, Expression right, BinaryOperationType operationType)
        {
            Left = left;
            Right = right;
            OperationType = operationType;
        }

        public override double Evaluate()
        {
            var left = Left.Evaluate();
            var right = Right.Evaluate();

            var result = OperationType switch
            {
                BinaryOperationType.Add => left + right,
                BinaryOperationType.Subtract => left - right,
                BinaryOperationType.Multiply => left * right,
                BinaryOperationType.Divide => left / right,
                BinaryOperationType.Power => Math.Pow(left, right),
                _ => throw new AstException("Unknown binary operation"),
            };

            return result;
        }

        public override string ToString()
        {
            char op = OperationType switch
            {
                BinaryOperationType.Add => '+',
                BinaryOperationType.Subtract => '-',
                BinaryOperationType.Multiply => '*',
                BinaryOperationType.Divide => '/',
                BinaryOperationType.Power => '^',
                _ => throw new AstException("Unknown binary operation"),
            };

            return $"({Left} {op} {Right})";
        }
    }
}
