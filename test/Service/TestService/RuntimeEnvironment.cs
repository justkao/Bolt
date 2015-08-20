using Microsoft.Dnx.Runtime;

namespace TestService.Client
{
    public class RuntimeEnvironment
    {
        public RuntimeEnvironment()
        {
        }

        public RuntimeEnvironment(IRuntimeEnvironment environment)
        {
            OperatingSystem = environment.OperatingSystem;
            OperatingSystemVersion = environment.OperatingSystemVersion;
            RuntimeType = environment.RuntimeType;
            RuntimeArchitecture = environment.RuntimeArchitecture;
            RuntimeVersion = environment.RuntimeVersion;
        }

        public string OperatingSystem { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string RuntimeType { get; set; }
        public string RuntimeArchitecture { get; set; }
        public string RuntimeVersion { get; set; }
    }
}