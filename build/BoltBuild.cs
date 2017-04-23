using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace build
{
    public class BoltBuild
    {
        private const string NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
        private const string NugetSourceUrl = "https://api.nuget.org/v3/index.json";
        
        private readonly string _root;

        private const string net451 = "net451";
        private const string netcoreapp1_0 = "netcoreapp1.1";


        public BoltBuild()
        {
            _root = Path.GetFullPath(Directory.GetCurrentDirectory());
        }

        public void Build()
        {
            Clean();
            RestoreProjects();
            BuildProjects();
            // TODO: fix contract generation
            // GenerateInterfaces();
            RunSample();
            RunQuickPerformanceTest(netcoreapp1_0);
            RunQuickPerformanceTest(net451);

            Test();
            PackPackages();
        }

        private void PackPackages()
        {
            foreach (string path in GetPaths(@"src\**\*.csproj"))
            {
                Execute("dotnet", $@"pack ""{path}"" --output ""{Path.GetFullPath(@"artifacts\Release")}"" --configuration RELEASE");
            }
        }

        public void Test()
        {
            foreach (string path in GetPaths(@"test\Automatic\**\*.csproj"))
            {
                try
                {
                    Execute("dotnet", $@"test ""{path}""");
                }
                catch (Exception)
                {
                    Console.WriteLine("Tests failed !!!!");
                }
            }

            foreach (string path in GetPaths(@"test\Integration\Bolt.Server.IntegrationTest\*.csproj"))
            {
                try
                {
                    Execute("dotnet", $@"test ""{path}""");
                }
                catch (Exception)
                {
                    Console.WriteLine("Tests failed !!!!");
                }
            }
        }

        public void BuildProjects()
        {		
            foreach (string path in GetPaths(@"src\**\*.csproj"))
            {
                Execute("dotnet", $"build {path}");
            }

            foreach (string path in GetPaths(@"test\**\*.csproj"))
            {
                Execute("dotnet", $"build {path}");
            }

            foreach (string path in GetPaths(@"samples\**\*.csproj"))
            {
                Execute("dotnet", $"build {path}");
            }

        }

        public void GenerateInterfaces()
        {
            string workingDirectory = @"test\Integration\Bolt.Server.IntegrationTest.Core\";
            string file = GetRequiredFile($@"{workingDirectory}**\Bolt.Server.IntegrationTest.Core.dll");
            Execute("dotnet",
                $@"bolt code ""{file}"" --contract ITestContract --output ..\Bolt.Server.IntegrationTest\Generated\",
                workingDirectory);
        }

        public IEnumerable<string> GetPaths(string pattern)
        {
            return GetPaths(null, pattern);
        }

        public IEnumerable<string> GetPaths(string folder, string pattern)
        {
            Matcher matcher = new Matcher();
            matcher.AddInclude(pattern);

            if (!string.IsNullOrEmpty(folder))
            {
                folder = Path.Combine(_root, folder);
            }
            else
            {
                folder = _root;
            }

            return matcher.GetResultsInFullPath(folder);
        }

        public string GetRequiredFile(string pattern)
        {
            var found = GetPaths(pattern).FirstOrDefault();
            if (found == null || !File.Exists(found))
            {
                throw new InvalidOperationException($@"File matching the pattern '{pattern}' not found.");
            }

            return found;
        }

        public IEnumerable<string> GetProjects()
        {
            return GetPaths(@"\**\*.csproj");
        }

        public void Clean()
        {
            try
            {
                Directory.Delete("artifacts", true);
            }
            catch (DirectoryNotFoundException)
            {
                // ok
            }

            Directory.CreateDirectory("artifacts");
        }

        public void RestoreProjects()
        {
            Execute("dotnet", " restore");
        }

        public void Execute(string fileName, string arguments, string workingDirectory = null)
        {
            Process process = StartExecute(fileName, arguments, workingDirectory);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to execute process - '{fileName} {arguments}'.");
            }
        }

        public Process StartExecute(string fileName, string arguments, string workingDirectory = null)
        {
            Console.WriteLine("Executing process: {0} {1}", fileName, arguments);

            Process process =
                Process.Start(new ProcessStartInfo(fileName, arguments)
                {
                    WorkingDirectory = Path.GetFullPath(workingDirectory ?? _root),
                    RedirectStandardInput = true
                });

            return process;
        }

        public void RunSample()
        {
            Execute("dotnet", $"run -p samples/Bolt.Sample.ContentProtection/Bolt.Sample.ContentProtection.csproj -f {net451}");
            Execute("dotnet", $"run -p samples/Bolt.Sample.ContentProtection/Bolt.Sample.ContentProtection.csproj -f {netcoreapp1_0}");
        }

        public void RunQuickPerformanceTest(string framework)
        {
            var serverProcess = StartExecute("dotnet", $"run -p test/Performance/Bolt.Performance.Server/Bolt.Performance.Server.csproj -f {framework}");
            System.Threading.Thread.Sleep(500);
            Execute("dotnet", $"run -f {framework} -p test/Performance/Bolt.Performance.Console/Bolt.Performance.Console.csproj quick");
            try
            {
                Console.WriteLine("Stopping the server ... ");
                serverProcess.StandardInput.Write("stop");
                serverProcess.WaitForExit();
                Console.WriteLine("Server stoppped");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to stop the server: {0}", e);
                // OK
            }
        }

        public void PublishBoltPackages()
        {
            Console.WriteLine("Downloading nuget ... ");
            HttpClient client = new HttpClient();
            byte[] nuget = client.GetAsync(NugetUrl).GetAwaiter().GetResult().Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            File.WriteAllBytes("artifacts/release/nuget.exe", nuget);
            try
            {

                foreach (string package in GetPaths("artifacts/release/*.nupkg").Where(p => !p.EndsWith("symbols.nupkg")))
                {
                    Console.WriteLine("Publishing {0} ... ", package);
                    Execute("artifacts/release/nuget.exe", $"push {Path.GetFileName(package)} -source {NugetSourceUrl}", "artifacts/release/");
                }
            }
            finally
            {
                // File.Delete("artifacts/release/nuget.exe");
            }
        }

        private string TryGetBuildNumber()
        {
            try
            {
                return Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}