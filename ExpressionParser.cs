using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RandomVariable
{
    public static class ExpressionParser
    {
        public static List<string> Parse(string mathExpression, Dictionary<string, Func<double, double, double>> operators)
        {
            var tempToken = "";
            var tokens = new List<string>();

            mathExpression = Regex.Replace(mathExpression, @"\+(\s*)\-","-");
            mathExpression = Regex.Replace(mathExpression, @"\-(\s*)\+","-");
            mathExpression = Regex.Replace(mathExpression, @"\-(\s*)\-","+");

            for (var i = 0; i < mathExpression.Length; i++)
            {
                var ch = mathExpression[i];

                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                if (char.IsDigit(ch))
                {
                    tempToken += ch;

                    while (i + 1 < mathExpression.Length && (char.IsDigit(mathExpression[i + 1]) || mathExpression[i + 1] == '.' || mathExpression[i + 1] == 'd'))
                    {
                        tempToken += mathExpression[++i];
                    }

                    tokens.Add(tempToken);
                    tempToken = "";

                    continue;
                }

                if (IsUnaryOperator(mathExpression, operators, i, ch, tokens))
                {
                    tempToken += ch;

                    while (i + 1 < mathExpression.Length && (char.IsDigit(mathExpression[i + 1]) || mathExpression[i + 1] == '.'))
                    {
                        tempToken += mathExpression[++i];
                    }

                    tokens.Add(tempToken);
                    tempToken = "";

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

        private static bool IsUnaryOperator(string mathExpression, Dictionary<string, Func<double, double, double>> operators, int i, char ch, List<string> tokens)
        {
            return i + 1 < mathExpression.Length &&
                   (ch == '-' || ch == '+') &&
                   char.IsDigit(mathExpression[i + 1]) &&
                   (i == 0 || (tokens.Count > 0 && operators.ContainsKey(tokens.Last())) || i - 1 > 0 && mathExpression[i - 1] == '(');
        }
    }
}
