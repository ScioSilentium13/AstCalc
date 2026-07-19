using AstCalculator.Core;
using AstCalculator.Core.Exceptions;
using AstDumper;
using System.Text;

namespace AstCalculatorCLI
{
    internal class Program
    {
        enum RunMode
        {
            Normal,
            Format,
            Dump
        }

        enum DumpMode
        {
            PlantUml
        }

        static void Main(string[] args)
        {
            PrintHello();
            Loop();
            PrintGoodbye();
        }

        static void Loop()
        {
            while (true)
            {
                Console.Write("> ");
                var input = ReadWithEsc();
                if (input == null)
                {
                    break;
                }

                input = input.Trim();
                var res = TryRunCommand(input);
                if (!res.HasValue) break;
                else if (res.Value) continue;

                RunCalculation(input, RunMode.Normal);
            }
        }

        static void RunCalculation(string input, RunMode mode, DumpMode dumpMode = DumpMode.PlantUml)
        {
            try
            {
                switch (mode)
                {
                    case RunMode.Normal:
                        {
                            var result = Parser.Calculate(input);
                            Console.WriteLine($"The result is: {result}");
                            break;
                        }
                    case RunMode.Format:
                        {
                            var result = Parser.Format(input);
                            Console.WriteLine($"The formatted expression is: {result}");
                            break;
                        }
                    case RunMode.Dump:
                        {
                            var expr = new Parser(input).Parse();
                            string result = "";
                            if (dumpMode == DumpMode.PlantUml)
                                result = new PlantUmlDumper().DumpAsString(expr);
                            Console.WriteLine();
                            Console.WriteLine(result);
                            break;
                        }
                    default: goto case RunMode.Normal;
                }
            }
            catch (AstException ex)
            {
                PrintError("Error while calculating!", ex.Message);
                if (ex.Position != null)
                    PrintPositionError(input, ex.Position.Value);
            }
            catch (Exception ex)
            {
                PrintError("Unknown error! ", ex.Message);
            }
        }

        static bool? TryRunCommand(string command)
        {
            if (command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                PrintHelp();
            }
            else if (command.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            else if (command.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.Clear();
            }
            else if (command.StartsWith("format ", StringComparison.InvariantCultureIgnoreCase))
            {
                command = command.Substring("format ".Length).Trim();
                RunCalculation(command, RunMode.Format);
            }
            else if (command.StartsWith("dump ", StringComparison.InvariantCultureIgnoreCase))
            {
                command = command.Substring("dump ".Length).Trim();
                var splitted = command.Split(' ');
                var type = splitted[0];
                if (type.Equals("plantuml", StringComparison.InvariantCultureIgnoreCase))
                {
                    RunCalculation(splitted[1], RunMode.Dump, DumpMode.PlantUml);
                }
                else
                {
                    PrintError("Unknown dump type! Check help.");
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static string? ReadWithEsc()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return null;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }

                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }

        static void PrintError(string message, string? ex_message = null)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ForegroundColor = color;
            if (ex_message != null)
            {
                Console.WriteLine($" {ex_message}");
            }
            else
            {
                Console.WriteLine();
            }                
        }
        static void PrintPositionError(string trimmed_input, int position)
        {
            Console.WriteLine(trimmed_input);
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new string(' ', position) + '↑');
            Console.ForegroundColor = color;
        }

        static void SlowerPrint(string msg, int delay = 50)
        {
            bool skip = false;
            foreach (char c in msg)
            {
                Console.Write(c);

                if (!skip)
                {
                    Thread.Sleep(delay);
                }

                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    skip = true;
                }
            }
        }

        static void PrintHello()
        {
            SlowerPrint("Welcome to Smart AST Calculator!");
            Thread.Sleep(100);
            SlowerPrint("\nJust enter your expression (or a command) and press Enter (or Esc to exit)!\n");
        }

        static void PrintHelp()
        {
            string help = """
                Supported operations:
                  - addition (+): a + b
                  - subtraction (-): a - b
                  - multiplication (*): a * b
                  - division (/): a / b
                  - power (^): a ^ b

                  - unary minus (-): -a
                  - parentheses: ((a + b) * c) - (d / e)

                Supported commands:
                  - help: prints this help
                  - format <expression>: formats the expression
                  - dump <format> <expression>: dumps the expression in the specified format
                    - plantuml: dumps the expression in PlantUML format
                  - clear: clears the console
                  - exit: exits the program
                """;
            SlowerPrint(help, 30);
            Console.WriteLine();
        }

        static void PrintGoodbye()
        {
            string msg = " >> GoOdByE << ";
            Random rnd = new();
            string glitch = "█▓▒░ ";
            int maxSteps = 40;
            int textSteps = 30;

            Console.CursorVisible = false;
            for (int i = 0; i <= maxSteps; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                var sb = new StringBuilder();

                foreach (char c in msg)
                {
                    if (i < textSteps)
                    {
                        if (i < 5 || rnd.Next(textSteps) > i)
                        {
                            if (rnd.Next(3) == 0)
                            {
                                if (char.IsUpper(c))
                                    sb.Append(char.ToLower(c));
                                else
                                    sb.Append(char.ToUpper(c));
                            }
                            else sb.Append(c);
                        }
                        else
                        {
                            sb.Append(glitch[rnd.Next(glitch.Length)]);
                        }
                    }
                    else
                    {
                        int fadeSteps = maxSteps - textSteps;
                        int progress = i - textSteps;

                        int baseIndex = (int)((progress / (double)fadeSteps) * (glitch.Length - 1));
                        baseIndex = Math.Clamp(baseIndex, 0, glitch.Length - 1);

                        double randomChance = (1.0 - progress / (double)fadeSteps) * 0.3;
                        if (rnd.NextDouble() < randomChance)
                        {
                            baseIndex = rnd.Next(glitch.Length);
                        }

                        sb.Append(glitch[baseIndex]);
                    }
                }

                Console.Write(sb.ToString());
                Thread.Sleep(100);
            }
            Thread.Sleep(300);
            Console.WriteLine();
            Console.CursorVisible = true;
        }
    }
}
