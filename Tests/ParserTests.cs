using AstCalculator.Core;
using AstCalculator.Core.Exceptions;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        // -------------------- Базовые арифметические операции --------------------
        [Test]
        public void TestAddition() =>
            Assert.That(Parser.Calculate("2 + 3"), Is.EqualTo(5));

        [Test]
        public void TestSubtraction() =>
            Assert.That(Parser.Calculate("10 - 4"), Is.EqualTo(6));

        [Test]
        public void TestMultiplication() =>
            Assert.That(Parser.Calculate("3 * 7"), Is.EqualTo(21));

        [Test]
        public void TestDivision() =>
            Assert.That(Parser.Calculate("15 / 3"), Is.EqualTo(5));

        [Test]
        public void TestDivisionByZero() =>
            Assert.That(Parser.Calculate("1 / 0"), Is.EqualTo(double.PositiveInfinity));

        // -------------------- Приоритет операций --------------------
        [Test]
        public void TestPriorityMultiplicationBeforeAddition() =>
            Assert.That(Parser.Calculate("2 + 3 * 4"), Is.EqualTo(14)); // 2 + 12

        [Test]
        public void TestPriorityDivisionBeforeSubtraction() =>
            Assert.That(Parser.Calculate("10 - 6 / 2"), Is.EqualTo(7)); // 10 - 3

        [Test]
        public void TestPriorityWithParentheses() =>
            Assert.That(Parser.Calculate("(2 + 3) * 4"), Is.EqualTo(20));

        // -------------------- Унарный минус --------------------
        [Test]
        public void TestUnaryMinusSimple() =>
            Assert.That(Parser.Calculate("-5"), Is.EqualTo(-5));

        [Test]
        public void TestUnaryMinusWithParentheses() =>
            Assert.That(Parser.Calculate("-(3 + 2)"), Is.EqualTo(-5));

        [Test]
        public void TestDoubleUnaryMinus() =>
            Assert.That(Parser.Calculate("--5"), Is.EqualTo(5));   // -(-5) = 5

        [Test]
        public void TestTripleUnaryMinus() =>
            Assert.That(Parser.Calculate("---5"), Is.EqualTo(-5)); // -(--5) = -5

        [Test]
        public void TestUnaryMinusAfterOperator() =>
            Assert.That(Parser.Calculate("5 * -3"), Is.EqualTo(-15));

        [Test]
        public void TestUnaryMinusAfterOperatorWithDoubleMinus() =>
            Assert.That(Parser.Calculate("5 * --3"), Is.EqualTo(15)); // 5 * 3

        [Test]
        public void TestUnaryMinusInSubtractionChain() =>
            Assert.That(Parser.Calculate("5 - -3"), Is.EqualTo(8));

        [Test]
        public void TestMultipleUnaryMinusesInChain() =>
            Assert.That(Parser.Calculate("5 --- 3"), Is.EqualTo(2));

        // -------------------- Возведение в степень --------------------
        [Test]
        public void TestPower() =>
            Assert.That(Parser.Calculate("2 ^ 3"), Is.EqualTo(8));

        [Test]
        public void TestPowerRightAssociativity() =>
            Assert.That(Parser.Calculate("2 ^ 3 ^ 2"), Is.EqualTo(512)); // 2^(3^2) = 2^9 = 512

        [Test]
        public void TestPowerWithUnaryMinus_BaseNegative() =>
            Assert.That(Parser.Calculate("-3 ^ 2"), Is.EqualTo(-9)); // -(3^2) = -9

        [Test]
        public void TestPowerWithUnaryMinus_BaseNegativeInParentheses() =>
            Assert.That(Parser.Calculate("(-3) ^ 2"), Is.EqualTo(9)); // (-3)^2 = 9

        [Test]
        public void TestPowerWithUnaryMinus_RightAssociative() =>
            Assert.That(Parser.Calculate("-3 ^ 2 ^ 3"), Is.EqualTo(-6561)); // -(3^(2^3)) = -(3^8) = -6561

        // -------------------- Десятичные числа и разделитель --------------------
        [Test]
        public void TestDecimalNumber() =>
            Assert.That(Parser.Calculate("3.14 + 2.86"), Is.EqualTo(6.0));

        [Test]
        public void TestDecimalWithLeadingDot() =>
            Assert.That(Parser.Calculate("0.5 * 2"), Is.EqualTo(1.0));

        [Test]
        public void TestCommaAsDecimalSeparator() =>
            Assert.That(Parser.Calculate("2,5 + 1,5"), Is.EqualTo(4.0)); // запятая заменяется на точку

        // -------------------- Пробелы и форматирование --------------------
        [Test]
        public void TestWhitespacesIgnored() =>
            Assert.That(Parser.Calculate("  2  +  3  "), Is.EqualTo(5));

        [Test]
        public void TestWhitespacesAroundUnaryMinus() =>
            Assert.That(Parser.Calculate("5 - - 3"), Is.EqualTo(8)); // пробелы между минусами

        // -------------------- Сложные комбинации --------------------
        [Test]
        public void TestComplexExpression1() =>
            Assert.That(Parser.Calculate("2 * (7 + 5 * (2 - 1)) - 4"), Is.EqualTo(20));

        [Test]
        public void TestComplexExpression2() =>
            Assert.That(Parser.Calculate("(3 + 5) ^ 2 - 4 * 6 / 2"), Is.EqualTo(64 - 12)); // 64 - 12 = 52

        [Test]
        public void TestComplexExpressionWithUnaryMinus() =>
            Assert.That(Parser.Calculate("-(5 + 3) * 2 ^ 3"), Is.EqualTo(-64)); // -8 * 8 = -64

        [Test]
        public void TestComplexExpressionNestedUnary() =>
            Assert.That(Parser.Calculate("--5 ^ 2"), Is.EqualTo(25));


        // -------------------- Ошибки (исключения) --------------------
        [Test]
        public void TestError_EmptyInput() =>
            Assert.That(() => Parser.Calculate(""), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_NullInput() =>
            Assert.That(() => Parser.Calculate(null!), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_OperatorAtEnd() =>
            Assert.That(() => Parser.Calculate("2 + 3 -"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_TwoOperatorsInRow() =>
            Assert.That(() => Parser.Calculate("2 ** 2"), Throws.InstanceOf<AstException>()); // два умножения

        [Test]
        public void TestError_UnbalancedParentheses_Open() =>
            Assert.That(() => Parser.Calculate("(2 + 3"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_UnbalancedParentheses_Close() =>
            Assert.That(() => Parser.Calculate("2 + 3)"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_EmptyParentheses() =>
            Assert.That(() => Parser.Calculate("()"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_OperatorAfterOpeningBracket() =>
            Assert.That(() => Parser.Calculate("(* 2)"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_UnknownSymbol() =>
            Assert.That(() => Parser.Calculate("2 # 3"), Throws.InstanceOf<AstException>());

        [Test]
        public void TestError_InvalidNumber() =>
            Assert.That(() => Parser.Calculate("2..3"), Throws.InstanceOf<AstException>()); // две точки подряд

        [Test]
        public void TestError_UnaryMinusAfterOperatorNotAllowedForOtherOperators() =>
            Assert.That(() => Parser.Calculate("5 * / 3"), Throws.InstanceOf<AstException>()); // */ не разрешено

        // -------------------- Проверка метода Format --------------------
        [Test]
        public void TestFormatSimple() =>
            Assert.That(Parser.Format("2 + 3"), Is.EqualTo("(2 + 3)"));

    }
}
