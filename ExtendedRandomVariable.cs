using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RandomVariable
{
    public class ExtendedRandomVariable
    {
        public string Value { get; }
        public double SidesCount { get; }
        public double DiceCount { get; }

        //public double ExpectedValue 
        //    => _expectedValue.Value;
        //public double Variance
        //    => _variance.Value;
        //public Dictionary<double, double> ProbabilityDistribution 
        //    => _probabilityDistribution.Value;
        

        //private readonly Lazy<double> _expectedValue;
        //private readonly Lazy<double> _variance;
        //private readonly Lazy<Dictionary<double, double>> _probabilityDistribution;

        public ExtendedRandomVariable(string value)
        {
            Value = value;
            DiceCount = Convert.ToDouble(value.Split('d')[0]);
            SidesCount = Convert.ToDouble(value.Split('d')[1]);

            //_expectedValue = new Lazy<double>(CalculateExpectedValue());
            //
            //_variance = new Lazy<double>(CalculateVariance());
            //
            //_probabilityDistribution = new Lazy<Dictionary<double, double>>(CalculateProbabilityDistribution(DiceCount, SidesCount));
        }
        
        public double CalculateExpectedValue() => DiceCount * (SidesCount + 1) / 2;

        public double CalculateVariance()
        {
            double sum = 0;
            for (double i = DiceCount; i <= SidesCount; i++)
            {
                sum += Math.Pow(i - (double)CalculateExpectedValue(), 2) / SidesCount;
            }

            return Math.Round(sum,2) * DiceCount;
        }

        public Dictionary<double,double> CalculateProbabilityDistribution()
        {
            var distribution = new Dictionary<double,double>();
            double count = SidesCount * DiceCount;
            double value = 0;
            for (var p = DiceCount; p <= count; p++)
            {
                double sum = 0;
                for (double k = 0; k <= (double)(p - DiceCount) / SidesCount; k++)
                {
                    sum += Math.Pow(-1, k) * C(DiceCount, k) * C(p - SidesCount * k - 1, DiceCount - 1);
                }

                if (DiceCount > 1 && distribution.Count > 0 && sum / Math.Pow(SidesCount, DiceCount) <= distribution.Last().Value)
                {
                    value = p;
                    break;
                }
                distribution[p] = sum / Math.Pow(SidesCount, DiceCount);
            }

            if (DiceCount <= 1) return distribution;

            var symmetricValues = new List<double>(distribution.Values);
            if ((count - DiceCount + 1) % 2 != 0)
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
