using System.Collections.Generic;

namespace RandomVariable
{
    public class RandomVariableStatistic
    {
        public virtual double? ExpectedValue { get; set; }
        public virtual double? Variance { get; set; }
        public virtual Dictionary<double, double> ProbabilityDistribution { get; set; }
    }
}