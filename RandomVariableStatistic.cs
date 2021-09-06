using System.Collections.Generic;

namespace RandomVariable
{
    public class RandomVariableStatistic
    {
        public virtual double? ExpectedValue { get; set; }
        public virtual double? Variance { get; set; }
        public virtual Dictionary<double, double> ProbabilityDistribution { get; set; }

        public RandomVariableStatistic()
        {
            ExpectedValue = null;
            Variance = null;
            ProbabilityDistribution = null;
        }

        public RandomVariableStatistic(double? expectedValue, double? variance, Dictionary<double, double> probabilityDistribution)
        {
            ExpectedValue = expectedValue;
            Variance = variance;
            ProbabilityDistribution = probabilityDistribution;
        }
    }
}