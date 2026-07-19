namespace AstCalculator.Core.Types
{
    /// <summary>
    /// Абстрактный класс для всех выражений в AST
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Вычисляет значения выражения
        /// </summary>
        /// <returns></returns>
        public abstract double Evaluate();

        public abstract override string ToString();
    }
}
