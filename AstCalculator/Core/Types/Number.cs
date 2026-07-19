namespace AstCalculator.Core.Types
{
    /// <summary>
    /// Представляет численную константу
    /// </summary>
    public class Number : Expression
    {
        private readonly double value;

        public Number(double value)
        {
            this.value = value;
        }

        public override double Evaluate()
        {
            return value;
        }

        public override string ToString()
        {
            return Evaluate().ToString();
        }
    }
}
