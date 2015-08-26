using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Bolt.Performance
{
    public class PerformanceResultHandler
    {
        public string GetReportFileName(string name, Version version)
        {
            return $"{name}_{DateTime.UtcNow.ToFileTimeUtc()}_{version}.json";
        }

        public PerformanceResult ReadLatestReport(string directory, int repeats, int concurrency)
        {
            return ReadReports(directory)
                .Where(r => r.Machine == Environment.MachineName && r.Concurrency == concurrency && r.Repeats == repeats)
                .OrderByDescending(r => r.Time)
                .FirstOrDefault();
        }

        public void WriteReportToDirectory(string directory, PerformanceResult result)
        {
            WriteReport(Path.Combine(directory, GetReportFileName("report", new Version(result.Version))), result);
        }

        public void WriteReport(string file, PerformanceResult result)
        {
            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.Formatting = Formatting.Indented;

            File.WriteAllText(file, JsonConvert.SerializeObject(result, settings));
        }

        public PerformanceResult ReadReport(string file)
        {
            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return JsonConvert.DeserializeObject<PerformanceResult>(File.ReadAllText(file), settings);
        }

        public PerformanceResult TryReadReport(string file)
        {
            try
            {
                return ReadReport(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<PerformanceResult> ReadReports(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return Enumerable.Empty<PerformanceResult>();
            }

            return Directory.GetFiles(directory).Select(TryReadReport).Where(r => r != null);
        }


    }
}