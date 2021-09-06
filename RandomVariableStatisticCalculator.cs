using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RandomVariable
{
    public class RandomVariableStatisticCalculator : IRandomVariableStatisticCalculator
    {
        public Dictionary<int,ExtendedRandomVariable> RandomVariables;

        public Dictionary<string, Func<double, double, double>> Operators { get; set; }
        public RandomVariableStatisticCalculator()
        {
            RandomVariables = new Dictionary<int, ExtendedRandomVariable>();
            Operators = new Dictionary<string, Func<double, double, double>>
            {
                ["/"] = (a, b) =>
                {
                    if (b != 0)
                        return a / b;
                    if (a > 0)
                        return double.PositiveInfinity;
                    if (a < 0)
                        return double.NegativeInfinity;
                    return double.NaN;
                },
                ["*"] = (a, b) => a * b,
                ["-"] = (a, b) => a - b,
                ["+"] = (a, b) => a + b,
            };
        }

        public RandomVariableStatistic CalculateStatistic(string expression, params StatisticKind[] statisticForCalculate)
        {
            var tokens = new ExpressionParser(Operators).Parse(expression);
            GetRandomVariables(tokens);
            var statistic = new RandomVariableStatistic();

            if (statisticForCalculate.Contains(StatisticKind.ExpectedValue))
            {
                var tokensForCalculateExpectedValue = tokens.Select((t, i) => RandomVariables.ContainsKey(i)
                        ? RandomVariables[i].CalculateExpectedValue().ToString(CultureInfo.InvariantCulture)
                        : tokens.ElementAt(i))
                    .ToList();
                statistic.ExpectedValue = CalculateExpectedValue(tokensForCalculateExpectedValue);
            }
            if(statisticForCalculate.Contains(StatisticKind.Variance))
            {

            }
            if (statisticForCalculate.Contains(StatisticKind.ProbabilityDistribution))
            {

            }



            return statistic;
        }
        private void GetRandomVariables(List<string> tokens)
        {
            var temp1 = tokens
                .Where(token => Regex.IsMatch(token, @"\d+d\d+"))
                .ToList();
                temp1.ForEach(rv =>RandomVariables[tokens.IndexOf(rv)] = new ExtendedRandomVariable(rv));
        }

        private double CalculateExpectedValue(List<string> tokens)
        {

            while (tokens.IndexOf("(") != -1)
            {
                var open = tokens.LastIndexOf("(");
                var close = tokens.IndexOf(")", open); 

                var roughExpr = new List<string>();

                for (var i = open + 1; i < close; i++)
                {
                    roughExpr.Add(tokens[i]);
                }

                double tmpResult;

                tmpResult = BasicArithmeticalExpression(roughExpr);

                tokens[open] = tmpResult.ToString(CultureInfo.InvariantCulture);
                tokens.RemoveRange(open + 1, close - open);

            }
            return BasicArithmeticalExpression(tokens);
        }

        private double BasicArithmeticalExpression(List<string> tokens)
        {
            double token0;
            double token1;

            switch (tokens.Count)
            {
                case 1:
                    if (!double.TryParse(tokens[0], NumberStyles.Number, CultureInfo.InvariantCulture, out token0))
                    {
                        throw new MathParserException("local variable " + tokens[0] + " is undefined");
                    }

                    return token0;
                case 2:
                    var op = tokens[0];

                    if (op == "-" || op == "+")
                    {
                        var first = op == "+" ? "" : (tokens[1].Substring(0, 1) == "-" ? "" : "-");

                        if (!double.TryParse(first + tokens[1], NumberStyles.Number, CultureInfo.InvariantCulture, out token1))
                        {
                            throw new MathParserException("local variable " + first + tokens[1] + " is undefined");
                        }

                        return token1;
                    }

                    if (!Operators.ContainsKey(op))
                    {
                        throw new MathParserException("operator " + op + " is not defined");
                    }

                    if (!double.TryParse(tokens[1], NumberStyles.Number, CultureInfo.InvariantCulture, out token1))
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
                    if (!double.TryParse(tokens[opPlace + 1], NumberStyles.Number, CultureInfo.InvariantCulture, out var rhs))
                    {
                        throw new MathParserException("local variable " + tokens[opPlace + 1] + " is undefined");
                    }

                    if (op.Key == "-" && opPlace == 0)
                    {
                        var result = op.Value(0.0, rhs);
                        tokens[0] = result.ToString(CultureInfo.InvariantCulture);
                        tokens.RemoveRange(opPlace + 1, 1);
                    }
                    else
                    {
                        if (!double.TryParse(tokens[opPlace - 1], NumberStyles.Number, CultureInfo.InvariantCulture, out var lhs))
                        {
                            throw new MathParserException("local variable " + tokens[opPlace - 1] + " is undefined");
                        }

                        var result = op.Value(lhs, rhs);
                        tokens[opPlace - 1] = result.ToString(CultureInfo.InvariantCulture);
                        tokens.RemoveRange(opPlace, 2);
                    }
                }
            }

            if (!double.TryParse(tokens[0], NumberStyles.Number, CultureInfo.InvariantCulture, out token0))
            {
                throw new MathParserException("local variable " + tokens[0] + " is undefined");
            }

            return token0;
        }

    }
}