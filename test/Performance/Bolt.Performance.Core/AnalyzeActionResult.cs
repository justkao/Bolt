namespace Bolt.Performance.Core
{
    public class AnalyzeActionResult
    {
        public PerformanceState State { get; set; }

        public string ActionName { get; set; }

        public double Value { get; set; }

        public long First { get; set; }

        public long Second { get; set; }

        public string GetPercentage()
        {
            return Value.ToString("P2");
        }
    }
}