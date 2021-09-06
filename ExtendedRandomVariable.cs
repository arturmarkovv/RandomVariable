using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RandomVariable
{
    public class ExtendedRandomVariable : RandomVariableStatistic
    {
        public string Value { get; }
        public double SidesCount { get; }
        public double DiceCount { get; }

        public override double? ExpectedValue 
            => _expectedValue.Value;
        public override Dictionary<double, double> ProbabilityDistribution 
            => _probabilityDistribution.Value;
        public override double? Variance 
            => _variance.Value;

        private readonly Lazy<double?> _expectedValue;
        private readonly Lazy<Dictionary<double, double>> _probabilityDistribution;
        private readonly Lazy<double?> _variance;
        public ExtendedRandomVariable(string value)
        {
            Value = value;
            DiceCount = Convert.ToDouble(value.Split('d')[0]);
            SidesCount = Convert.ToDouble(value.Split('d')[1]);

            _expectedValue = new Lazy<double?>(CalculateExpectedValue());

            _variance = new Lazy<double?>(CalculateVariance());

            _probabilityDistribution = new Lazy<Dictionary<double, double>>(ProbabilityDistributionDensity(DiceCount, SidesCount));
        }

        public double CalculateExpectedValue() => DiceCount * (SidesCount + 1) / 2;

        public double CalculateVariance()
        {
            double sum = 0;
            for (double i = DiceCount; i <= SidesCount; i++)
            {
                sum += Math.Pow(i - (double)ExpectedValue, 2) / SidesCount;
            }

            return Math.Round(sum,2) * DiceCount;
        }

        public Dictionary<double,double> ProbabilityDistributionDensity(double diceCount, double sidesCount)
        {
            var distribution = new Dictionary<double,double>();
            double count = sidesCount * diceCount;
            double value = 0;
            for (var p = diceCount; p <= count; p++)
            {
                double sum = 0;
                for (double k = 0; k <= (double)(p - diceCount) / sidesCount; k++)
                {
                    sum += Math.Pow(-1, k) * C(diceCount, k) * C(p - sidesCount * k - 1, diceCount - 1);
                }

                if (diceCount > 1 && distribution.Count > 0 && sum / Math.Pow(sidesCount, diceCount) <= distribution.Last().Value)
                {
                    value = p;
                    break;
                }
                distribution[p] = sum / Math.Pow(sidesCount, diceCount);
            }

            if (diceCount <= 1) return distribution;

            var symmetricValues = new List<double>(distribution.Values);
            if ((count - diceCount + 1) % 2 != 0)
            {
                symmetricValues.RemoveAt(symmetricValues.Count - 1);
            }
            symmetricValues.Reverse();
            symmetricValues.ForEach(x => distribution.Add(++value, x));

            return distribution;


            static double C(double up, double down)
            {
                static BigInteger Factor(BigInteger n)
                {
                    BigInteger result = 1;
                    for (BigInteger i = 2; i <= n; i++)
                    {
                        result *= i;
                    }

                    return result;
                }
                return (double)(Factor((BigInteger)up) / (Factor((BigInteger)down) * Factor((BigInteger)(up - down))));
            }
        }
    }
}
