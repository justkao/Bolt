using System;
using System.Collections.Generic;

namespace Bolt.Performance.Core
{
    public class PerformanceResult
    {
        public PerformanceResult()
        {
            Time = DateTime.UtcNow;
            Machine = System.Environment.MachineName;
            Cores = System.Environment.ProcessorCount;
            Actions = new Dictionary<string, ActionMetadata>();
        }

        public string Version { get; set; }

        public int Concurrency { get; set; }

        public int Repeats { get; set; }

        public string Machine { get; set; }

        public int Cores { get; set; }

        public DateTime Time { get; set; }

        public SerializableRuntimeEnvironment Environment { get; set; }

        public Dictionary<string, ActionMetadata> Actions { get; set; }
    }
}