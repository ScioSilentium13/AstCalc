using AstCalculator.Core.Types;
using System.Text;

namespace AstDumper
{
    public class PlantUmlDumper : IAstDumper
    {
        private int _id = 0;
        public string DumpAsString(Expression expression)
        {
            _id = 0;

            StringBuilder sb = new();
            sb.AppendLine("@startuml");
            sb.AppendLine("skinparam objectStyle rectangle"); 
            sb.AppendLine("hide empty fields");
            DumpExpression(expression, sb);
            sb.AppendLine("@enduml");
            return sb.ToString();
        }

        private string DumpExpression(Expression expression, StringBuilder sb)
        {
            string id = $"n{_id++}";
            string label = expression switch
            {
                Number n => $"Number\\n{n.Evaluate()}",
                BinaryOperation b => $"BinaryOperation\\n{b.OperationType}",
                UnaryOperation u => $"UnaryOperation\\n{u.OperationType}",
                ParenthesizedExpression => $"ParenthesizedExpression",
                _ => throw new NotImplementedException()
            };

            sb.AppendLine($"object \"{label}\" as {id}");

            switch (expression)
            {
                case BinaryOperation b:
                    var leftId = DumpExpression(b.Left, sb);
                    var rightId = DumpExpression(b.Right, sb);

                    sb.AppendLine($"{id} --> {leftId} : Left");
                    sb.AppendLine($"{id} --> {rightId} : Right");
                    break;

                case UnaryOperation u:
                    var childId = DumpExpression(u.Expression, sb);
                    sb.AppendLine($"{id} --> {childId}");
                    break;

                case ParenthesizedExpression p:
                    var childId2 = DumpExpression(p.Expression, sb);
                    sb.AppendLine($"{id} --> {childId2}");
                    break;
            }

            return id;
        }
    }
}
