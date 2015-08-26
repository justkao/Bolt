using System.Collections.Generic;

namespace Bolt.Performance
{
    public class ActionMetadata
    {
        public ActionMetadata()
        {
            Metrics = new Dictionary<string, long>();
        }

        public Dictionary<string, long> Metrics { get; set; }

        public AnalyzeActionResult Analyze(ActionMetadata other, string actionName)
        {
            if (!Metrics.ContainsKey(actionName) || !other.Metrics.ContainsKey(actionName))
            {
                return null;
            }

            var currentValue = Metrics[actionName];
            var otherValue = other.Metrics[actionName];

            if (currentValue < otherValue)
            {
                return new AnalyzeActionResult
                           {
                               State = PerformanceState.Improvement,
                               ActionName = actionName,
                               Value = CalculatePercentage(otherValue, currentValue),
                               First = currentValue,
                               Second = otherValue
                           };
            }
            if (currentValue > otherValue)
            {
                return new AnalyzeActionResult
                           {
                               State = PerformanceState.Regression,
                               ActionName = actionName,
                               Value = CalculatePercentage(currentValue, otherValue),
                               First = currentValue,
                               Second = otherValue
                           };
            }

            return new AnalyzeActionResult
                       {
                           State = PerformanceState.Unchanged,
                           ActionName = actionName,
                           Value = 0,
                           First = currentValue,
                           Second = otherValue
                       };
        }

        private static double CalculatePercentage(long current, long previous)
        {
            return (1.0 - previous / (double)current);
        }
    }
}