using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RandomVariable
{
    public class ExpressionParser
    {
        public Dictionary<string, Func<double, double, double>> Operators { get; set; }
        public Dictionary<string, (double,double,double[])> RandomValues { get; set; }

        public CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        public ExpressionParser()
        {
            Operators = new Dictionary<string, Func<double, double, double>>
            {
                ["/"] = (a, b) =>
                {
                    if (b != 0)
                        return a / b;
                    else if (a > 0)
                        return double.PositiveInfinity;
                    else if (a < 0)
                        return double.NegativeInfinity;
                    else
                        return double.NaN;
                },
                ["*"] = (a, b) => a * b,
                ["-"] = (a, b) => a - b,
                ["+"] = (a, b) => a + b,
            };
            RandomValues = new Dictionary<string, (double, double, double[])>();
        }

        public double Parse(string mathExpression)
        {
            return MathParserLogic(Lexer(mathExpression));
        }

        private List<string> Lexer(string mathExpression)
        {
            var token = "";
            var tokens = new List<string>();

            mathExpression = mathExpression.Replace("+-", "-");
            mathExpression = mathExpression.Replace("-+", "-");
            mathExpression = mathExpression.Replace("--", "+");

            for (var i = 0; i < mathExpression.Length; i++)
            {
                var ch = mathExpression[i];

                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                if (char.IsLetter(ch))
                {
                    if (i != 0 && (char.IsDigit(mathExpression[i - 1]) || mathExpression[i - 1] == ')'))
                    {
                        tokens.Add("*");
                    }

                    token += ch;

                    while (i + 1 < mathExpression.Length && char.IsLetterOrDigit(mathExpression[i + 1]))
                    {
                        token += mathExpression[++i];
                    }

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (char.IsDigit(ch))
                {
                    token += ch;

                    while (i + 1 < mathExpression.Length && (char.IsDigit(mathExpression[i + 1]) || mathExpression[i + 1] == '.'))
                    {
                        token += mathExpression[++i];
                    }

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (ch == '.')
                {
                    token += ch;

                    while (i + 1 < mathExpression.Length && char.IsDigit(mathExpression[i + 1]))
                    {
                        token += mathExpression[++i];
                    }

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (i + 1 < mathExpression.Length &&
                    (ch == '-' || ch == '+') &&
                    char.IsDigit(mathExpression[i + 1]) &&
                    (i == 0 || (tokens.Count > 0 && Operators.ContainsKey(tokens.Last())) || i - 1 > 0 && mathExpression[i - 1] == '('))
                {
                    // if the above is true, then the token for that negative number will be "-1", not "-","1".
                    // to sum up, the above will be true if the minus sign is in front of the number, but
                    // at the beginning, for example, -1+2, or, when it is inside the brakets (-1), or when it comes after another operator.
                    // NOTE: this works for + as well!

                    token += ch;

                    while (i + 1 < mathExpression.Length && (char.IsDigit(mathExpression[i + 1]) || mathExpression[i + 1] == '.'))
                    {
                        token += mathExpression[++i];
                    }

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (ch == '(')
                {
                    if (i != 0 && (char.IsDigit(mathExpression[i - 1]) || char.IsDigit(mathExpression[i - 1]) || mathExpression[i - 1] == ')'))
                    {
                        tokens.Add("*");
                        tokens.Add("(");
                    }
                    else
                    {
                        tokens.Add("(");
                    }
                }
                else
                {
                    tokens.Add(ch.ToString());
                }
            }

            return tokens;
        }

        private double MathParserLogic(object lexer)
        {
            throw new NotImplementedException();
        }

        private double BasicArithmeticalExpression(List<string> tokens)
        {
            double token0;
            double token1;

            switch (tokens.Count)
            {
                case 1:
                    if (!double.TryParse(tokens[0], NumberStyles.Number, CultureInfo, out token0))
                    {
                        throw new MathParserException("local variable " + tokens[0] + " is undefined");
                    }

                    return token0;
                case 2:
                    var op = tokens[0];

                    if (op == "-" || op == "+")
                    {
                        var first = op == "+" ? "" : (tokens[1].Substring(0, 1) == "-" ? "" : "-");

                        if (!double.TryParse(first + tokens[1], NumberStyles.Number, CultureInfo, out token1))
                        {
                            throw new MathParserException("local variable " + first + tokens[1] + " is undefined");
                        }

                        return token1;
                    }

                    if (!Operators.ContainsKey(op))
                    {
                        throw new MathParserException("operator " + op + " is not defined");
                    }

                    if (!double.TryParse(tokens[1], NumberStyles.Number, CultureInfo, out token1))
                    {
                        throw new MathParserException("local variable " + tokens[1] + " is undefined");
                    }

                    return Operators[op](0, token1);
                case 0:
                    return 0;
            }

            foreach (var op in Operators)
            {
                int opPlace;

                while ((opPlace = tokens.IndexOf(op.Key)) != -1)
                {
                    double rhs;

                    if (!double.TryParse(tokens[opPlace + 1], NumberStyles.Number, CultureInfo, out rhs))
                    {
                        throw new MathParserException("local variable " + tokens[opPlace + 1] + " is undefined");
                    }

                    if (op.Key == "-" && opPlace == 0)
                    {
                        var result = op.Value(0.0, rhs);
                        tokens[0] = result.ToString(CultureInfo);
                        tokens.RemoveRange(opPlace + 1, 1);
                    }
                    else
                    {
                        double lhs;

                        if (!double.TryParse(tokens[opPlace - 1], NumberStyles.Number, CultureInfo, out lhs))
                        {
                            throw new MathParserException("local variable " + tokens[opPlace - 1] + " is undefined");
                        }

                        var result = op.Value(lhs, rhs);
                        tokens[opPlace - 1] = result.ToString(CultureInfo);
                        tokens.RemoveRange(opPlace, 2);
                    }
                }
            }

            if (!double.TryParse(tokens[0], NumberStyles.Number, CultureInfo, out token0))
            {
                throw new MathParserException("local variable " + tokens[0] + " is undefined");
            }

            return token0;
        }
    }
}
