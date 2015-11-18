using Bolt.Client;
using Microsoft.Extensions.PlatformAbstractions;

namespace Bolt.Performance.Console
{
    public static class PerformanceExtensions
    {
        public static void UpdateVersion(this PerformanceResult source)
        {
            source.Version = typeof(IProxy).Assembly.GetName().Version.ToString();
        }

        public static void Update(this RuntimeEnvironment source, IRuntimeEnvironment environment)
        {
            source.OperatingSystem = environment.OperatingSystem;
            source.OperatingSystemVersion = environment.OperatingSystemVersion;
            source.RuntimeArchitecture = environment.RuntimeArchitecture;
            source.RuntimeType = environment.RuntimeType;
            source.RuntimeVersion = environment.RuntimeVersion;
        }
    }
}