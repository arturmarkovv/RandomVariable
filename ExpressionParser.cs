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
            var token = "";
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

                    while (i + 1 < mathExpression.Length && (char.IsDigit(mathExpression[i + 1]) || mathExpression[i + 1] == '.' || mathExpression[i + 1] == 'd'))
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
                if (ch == 'd')
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
                    (i == 0 || (tokens.Count > 0 && operators.ContainsKey(tokens.Last())) || i - 1 > 0 && mathExpression[i - 1] == '('))
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
    }
}
