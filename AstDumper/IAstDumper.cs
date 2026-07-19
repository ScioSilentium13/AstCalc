using AstCalculator.Core.Types;

namespace AstDumper
{
    public interface IAstDumper
    {
        public string DumpAsString(Expression expression);
    }
}
