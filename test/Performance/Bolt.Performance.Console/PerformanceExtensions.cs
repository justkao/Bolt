using System.Reflection;
using System.Runtime.InteropServices;
using Bolt.Client;
using Bolt.Performance.Core;

namespace Bolt.Performance.Console
{
    public static class PerformanceExtensions
    {
        public static void UpdateVersion(this PerformanceResult source)
        {
            source.Version = typeof(IProxy).GetTypeInfo().Assembly.GetName().Version.ToString();
        }

        public static void Update(this SerializableRuntimeEnvironment source)
        {
            source.OSDescription = RuntimeInformation.OSDescription;
            source.ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
            source.OSArchitecture = RuntimeInformation.OSArchitecture.ToString();
            source.FrameworkDescription = RuntimeInformation.FrameworkDescription;
        }
    }
}