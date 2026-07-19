using AstCalculator.Core.Exceptions;
using AstCalculator.Core.Types;
using System.Globalization;
using System.Text;
using Expression = AstCalculator.Core.Types.Expression;

namespace AstCalculator.Core
{
    public class Parser
    {
        private readonly string _expression;

        /// <summary>
        /// Позиция следующего символа
        /// </summary>
        private int _position;

        public Parser(string expression)
        {
            _expression = SanitizeInput(expression);
            _position = 0;
        }

        #region Вспомогательные методы
        /// <summary>
        /// Пропускает все пробелы и сдвигает позицию на следующий непробельный символ
        /// </summary>
        private void SkipWhitespaces()
        {
            while (_position < _expression.Length && char.IsWhiteSpace(_expression[_position]))
            {
                _position++;
            }
        }

        /// <summary>
        /// Возвращает следующий символ без сдвига позиции
        /// </summary>
        /// <returns><see cref="char"/>, иначе (конец строки или если символ вне диапазона) '\0'</returns>
        private char Peek()
        {
            SkipWhitespaces();
            return _position < _expression.Length ? _expression[_position] : '\0';
        }


        /// <summary>
        /// Возвращает следующий символ и сдвигает позицию
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AstException"></exception>
        private char Next()
        {
            SkipWhitespaces();
            if (_position >= _expression.Length)
                throw new AstException($"Char position out of range - {_position}");

            return _expression[_position++];
        }

        /// <summary>
        /// Парсит выражение в AST и возвращает корневой узел
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AstException"></exception>
        public Expression Parse()
        {
            //Начинаем с самого низкого приоритета, он сам спустится
            //вниз к самому высокому рекурсивно
            var expr = ParseExpression();

            //Проверка, что всё выражение распарсилось:
            //пропускаем пробелы и убеждаемся, что ничего после
            //не осталось - мы дошли до конца строки
            SkipWhitespaces();
            if (_position < _expression.Length)
            {
                throw new AstException($"Parsing stopped at position {_position}", _position);
            }

            return expr;
        }

        /// <summary>
        /// Чистит и подготавливает строку к парсингу
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="AstException"></exception>
        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new AstException("Input is empty");

            const string single_marks = "+-*/\\^,.";
            const string double_marks = "()";
            input = input.Trim();

            StringBuilder sb = new();

            bool lastWasWhitespace = false;
            bool lastWasOperator = false;
            bool lastWasOpenedBracket = false;
            int openedBrackets = 0;
            int closedBrackets = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (char.IsDigit(c))
                {
                    lastWasWhitespace = false;
                    lastWasOperator = false;
                    lastWasOpenedBracket = false;
                    sb.Append(c);
                }
                else if (double_marks.Contains(c))
                {
                    if (c == '(')
                    {
                        lastWasOpenedBracket = true;
                        openedBrackets++;
                    }
                    else if (c == ')')
                    {
                        if (lastWasOpenedBracket)
                        {
                            throw new AstException($"Empty brackets at position {i}", i);
                        }

                        if (lastWasOperator)
                        {
                            throw new AstException($"Operator before closing bracket at position {i}", i);
                        }

                        lastWasOpenedBracket = false;
                        closedBrackets++;
                    }

                    lastWasOperator = false;
                    lastWasWhitespace = false;
                    sb.Append(c);
                }
                else if (single_marks.Contains(c))
                {
                    lastWasWhitespace = false;

                    if (lastWasOperator)
                    {
                        if (c == '-')
                        {
                            lastWasOpenedBracket = false;
                            sb.Append(c);
                            continue;
                        }
                        else
                            throw new AstException($"More then one single mark in a row at position {i}", i);
                    }

                    if (lastWasOpenedBracket && c != '-')
                    {
                        throw new AstException($"Operator after opening bracket at position {i}", i);
                    }

                    lastWasOpenedBracket = false;
                    lastWasOperator = true;

                    sb.Append(c == ',' ? '.' : c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (lastWasWhitespace)
                        continue;
                    else
                    {
                        lastWasWhitespace = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    throw new AstException($"Unknown char '{c}' ({(int)c}) at position {i}", i);
                }
            }

            if (openedBrackets != closedBrackets)
            {
                throw new AstException($"Opened brackets ({openedBrackets}) and closed brackets ({closedBrackets}) are not equal");
            }

            if (sb.Length > 0 && single_marks.Contains(sb[^1]))
                throw new AstException("Input ends with operator", sb.Length - 1);

            return sb.ToString();
        }
        #endregion

        #region Парсеры

        //Приоритеты: 1 - самый высокий (вычисляется первым), 4 - самый низкий (вычисляется последним)

        /// <summary>
        /// Парсит Factor - числа и скобки (приоритет 1)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AstException"></exception>
        private Expression ParseFactor()
        {
            //Проверяем, есть ли унарный минус перед выражением
            //и сразу собираем все минусы, сокращая их
            int negCount = 0;
            while (Peek() == '-')
            {
                negCount++;
                Next();
            }

            //Если минусов нечетное количество, то число отрицательное
            bool isNegative = negCount % 2 == 1;

            char c = Peek();

            //Если это просто число (константа) - определили по первой цифре
            if (char.IsDigit(c))
            {
                //Начинаем парсить число, будем собирать его посимвольно
                StringBuilder sb = new();

                //Сразу добавляем первую цифру
                sb.Append(c);

                //Пропускаем её...
                Next();

                //...и пикаем следующий символ
                c = Peek();

                //Парсим, пока выражение не закончилось,
                //и следующий символ - цифра, точка или запятая
                while (c != '\0' && (char.IsDigit(c) || c == '.' || c == ','))
                {
                    sb.Append(c);
                    Next();
                    c = Peek();
                }
                
                var string_value = sb.ToString();

                //Пытаемся из строки получить число...
                if (!double.TryParse(string_value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double result))
                {
                    //...не получилось 😭
                    throw new AstException($"Unable to parse '{string_value}' as double");
                }

                //Создаем числовой узел AST
                Expression expr = new Number(result);

                //Если до скобок стоял унарный минус...
                if (isNegative)
                {
                    //...применяем отрицание
                    expr = new UnaryOperation(expr, UnaryOperationType.Negate);
                }

                return expr;
            }

            //Если это выражение (в скобках)
            if (c == '(')
            {
                //Пропускаем скобку
                Next();

                //Парсим вложенное выражение
                var expr = ParseExpression();

                //Если нет закрывающей скобки, ошибка
                if (Peek() != ')')
                {
                    throw new AstException("Closing bracket is missing");
                }

                //Пропускаем скобку
                Next();

                //Если до скобок стоял унарный минус...
                if (isNegative)
                {
                    //...то применяем к выражению унарную операцию отрицания
                    expr = new UnaryOperation(expr, UnaryOperationType.Negate);
                }

                //Так как мы парсили выражение в скобках, то создаем
                //соответствующий узел-обертку. По сути, это нужно
                //только для правильного парсинга степеней, поскольку
                //приоритет унарного минуса основания у (-a)^b и -a^b
                //разный. В первом случае нужно сначала применять минус 
                //к основанию, а во втором наоборот - в самом конце.
                //Без этой обёртки первый будет действовать как второй
                //и правильно работать не будет.
                return new ParenthesizedExpression(expr);
            }

            throw new AstException($"Unknown char: '{c}'");
        }

        /// <summary>
        /// Парсит Power - возведение в степень (приоритет 2)
        /// </summary>
        /// <returns></returns>
        private Expression ParsePower()
        {
            //NOTE: Другие методы парсера работают похожим (и гораздо более простым)
            //образом, поэтому нет смысла объяснять их в дополнение к этому.

            //Так как у Factor приоритет выше, парсим сначала его, например число
            var left = ParseFactor();

            bool isNegative = false;
            while (true)
            {
                //Пикаем следующий символ (после числа, которое вы считали выше)
                var op = Peek();

                //Если это не оператор степени, выходим из цикла
                if (op != '^')
                    break;

                //Иначе, пропускаем символ степени
                Next();

                //Во всех других парсерах (Term и Expression), мы одинаково парсили
                //левый и правый операнд. Это из-за левой ассоциативности.
                //Но здесь левый (Factor) и правый (Power) отличаются,
                //потому что нам нужно сначала собрать все степени справа (это важно),
                //а уже потом считать первую (самую левую) степень в выражении.
                //В других парсерах это неявно, потому что по умолчанию наш AST собирается
                //как раз слева направо, а степени должны вычисляться наоборот.
                //Например, для умножения приоритет (скобки) будет установлен так:
                //a * b * c -> ((a * b) * c)
                //Слева направо, то есть левая ассоциативность.
                //А для степеней необходимо так:
                //a ^ b ^ c -> (a ^ (b ^ c))
                //Справа налево, то есть правая ассоциативность.
                //То есть, мы считаем сначала степень; это имеет смысл, если у
                //самой степени есть степень. Так мы идем от самой дальней степени
                //к первой. Это свойство степеней, и без этого несколько степеней подряд
                //будут считаться неправильно.
                var right = ParsePower();

                //Этот код тоже является следствием свойств степеней.
                //Если для левоассоциативных операций унарный минус применяется сразу
                //к первому операнду, и потом считается всё остальное слева направо
                //(и минус уже изменил знак числа), то со степенями по-другому.
                //Из-за их правой ассоциативности, унарный минус должен применяться
                //в конце вычислений, и не к самому основанию, а к результату возведения в степень.
                //Пример:
                //1) Без этого кода (неправильно): -3 ^ 2 -> (-3) ^ 2 = 9
                //2) С этим кодом (правильно): -3 ^ 2 -> -(3 ^ 2) = -9
                //По сути, всё, что делает этот код, это проверяет, имеет ли левый операнд
                //унарный минус, и, если это так,
                //он его удаляет, запоминает, и применяет позже к общему результату.
                if (left is UnaryOperation uo && uo.OperationType == UnaryOperationType.Negate)
                {
                    left = uo.Expression;
                    isNegative = true;
                }

                //Собираем выражение: левый операнд, правый операнд и оператор (степень)
                left = new BinaryOperation(left, right, BinaryOperationType.Power);
            }

            //Если до возведения в степень стоял унарный минус у левого операнда...
            if (isNegative)
            {
                //...то применяем его ко всему выражению унарную операцию отрицания
                return new UnaryOperation(left, UnaryOperationType.Negate);
            }

            //Если код дошел до этой строчки, не входя в цикл, это значит,
            //что в выражении отсутвует степень. И, соответственно, мы возвращаем то,
            //что мы уже спарсили - Factor.
            //Важно! Не во всём (исходном) выражении нет степеней, а только в той
            //части, которую получил этот метод. А так как этот метод всегда вызывает
            //ParseTerm (а его другие) для разбора части выражения на наличие степеней
            //(если их нет, то этот метод вернет то, что сам искал внутри - Factor),
            //то так должно быть часто.
            return left;
        }

        /// <summary>
        /// Парсит Term - умножение и деление (приоритет 3)
        /// </summary>
        /// <returns></returns>
        private Expression ParseTerm()
        {
            //Ищем степень, так как у нее приоритет на 1 выше, чем у Term
            var left = ParsePower();

            while (true)
            {
                var op = Peek();
                if (op != '*' && op != '/' && op != '\\')
                    break;

                Next();
                var right = ParsePower();
                var opType = op == '*' ? BinaryOperationType.Multiply : BinaryOperationType.Divide;
                left = new BinaryOperation(left, right, opType);
            }

            return left;
        }

        /// <summary>
        /// Парсит Expression - сложение и вычитание (приоритет 4)
        /// </summary>
        /// <returns></returns>
        private Expression ParseExpression()
        {
            //Умножение главнее сложения и вычитания
            var left = ParseTerm();

            while (true)
            {
                var op = Peek();
                if (op != '+' && op != '-')
                    break;

                Next();
                var right = ParseTerm();
                var opType = op == '+' ? BinaryOperationType.Add : BinaryOperationType.Subtract;
                left = new BinaryOperation(left, right, opType);
            }

            return left;
        }
        #endregion

        /// <summary>
        /// Удобная обертка для вычисления значения выражения
        /// </summary>
        public static double Calculate(string expression)
        {
            return new Parser(expression).Parse().Evaluate();
        }

        /// <summary>
        /// Удобная обертка для форматирования выражения
        /// </summary>
        public static string Format(string expression)
        {
            return new Parser(expression).Parse().ToString();
        }
    }
}
