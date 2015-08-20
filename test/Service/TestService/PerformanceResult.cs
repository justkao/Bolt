using System;
using System.Collections.Generic;
using Bolt.Client;
using Newtonsoft.Json;

namespace TestService.Client
{
    public class PerformanceResult
    {
        public PerformanceResult()
        {
            Time = DateTime.UtcNow;
            Version = typeof (IProxy).Assembly.GetName().Version.ToString();
            Machine = System.Environment.MachineName;
            Cores = System.Environment.ProcessorCount;
        }

        public string Version { get; set; }

        public int Concurrency { get; set; }

        public int Repeats { get; set; }

        public string Machine { get; set; }

        public int Cores { get; set; }

        public DateTime Time { get; set; }

        public RuntimeEnvironment Environment { get; set; }

        public Dictionary<string, Dictionary<string, long>> Actions { get; set; }

    }
}