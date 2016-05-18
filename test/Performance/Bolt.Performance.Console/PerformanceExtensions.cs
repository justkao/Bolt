using System.Reflection;
using Bolt.Client;
using Bolt.Performance.Core;
using Microsoft.Extensions.PlatformAbstractions;
using RuntimeEnvironment = Microsoft.Extensions.PlatformAbstractions.RuntimeEnvironment;

namespace Bolt.Performance.Console
{
    public static class PerformanceExtensions
    {
        public static void UpdateVersion(this PerformanceResult source)
        {
            source.Version = typeof(IProxy).GetTypeInfo().Assembly.GetName().Version.ToString();
        }

        public static void Update(this SerializableRuntimeEnvironment source, RuntimeEnvironment environment)
        {
            source.OperatingSystem = environment.OperatingSystem;
            source.OperatingSystemVersion = environment.OperatingSystemVersion;
            source.RuntimeArchitecture = environment.RuntimeArchitecture;
            source.RuntimeType = environment.RuntimeType;
            source.RuntimeVersion = environment.RuntimeVersion;
        }
    }
}