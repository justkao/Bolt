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
            return $"{name}_{version}.json";
        }

        public Version ParseVersion(string name)
        {
            Version version;
            Version.TryParse(Path.GetFileNameWithoutExtension(name).Split('_').Last(), out version);
            return version;
        }

        public PerformanceResult ReadLatestReport(string directory, Version excludedVersion)
        {
            var found = GetLatestReportVersion(directory, excludedVersion);
            if (Equals(found, default(KeyValuePair<string,Version>)))
            {
                return null;
            }

            return ReadReport(found.Key);
        }

        public PerformanceResult ReadReport(string file)
        {
            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return JsonConvert.DeserializeObject<PerformanceResult>(File.ReadAllText(file), settings);
        }

        public KeyValuePair<string, Version> GetLatestReportVersion(string directory, Version excludedVersion)
        {
            return ReadReportVersions(directory).FirstOrDefault(p => p.Value != excludedVersion);
        }

        public IEnumerable<PerformanceResult> ReadReports(string directory)
        {
            return ReadReportVersions(directory).Select(v => ReadReport(v.Key));
        }

        public IEnumerable<KeyValuePair<string, Version>> ReadReportVersions(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return Enumerable.Empty<KeyValuePair<string, Version>>();
            }

            return from report in Directory.GetFiles(directory)
                   let version = ParseVersion(report)
                   where version != null
                   orderby version descending
                   select new KeyValuePair<string, Version>(report, version);
        }
    }
}