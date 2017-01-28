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
        
        private const string BoltVersion = "0.60.0-alpha1";
        private readonly string _root;

        public BoltBuild()
        {
            _root = Path.GetFullPath(Directory.GetCurrentDirectory());
        }

        public void Build()
        {
            // Clean();
            // RestoreProjects();
            BuildProjects();
            // TODO: fix contract generation
            // GenerateInterfaces();
            RunSample();
            RunQuickPerformanceTest();
            Test();
            UpdateBoltVersion();
            PackPackages();
        }

        private void PackPackages()
        {
            foreach (string path in GetPaths(@"src\**\*.csproj"))
            {
                Execute("dotnet", $@"pack ""{path}"" --output artifacts\Release --configuration RELEASE");
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

            foreach (string path in GetPaths(@"test\Integration\**\*.csproj"))
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
            Execute("dotnet", "run -p samples/Bolt.Sample.ContentProtection");
        }

        public void RunQuickPerformanceTest()
        {
            var serverProcess = StartExecute("dotnet", "run -p test/Performance/Bolt.Performance.Server");
            System.Threading.Thread.Sleep(500);
            Execute("dotnet", "run -p test/Performance/Bolt.Performance.Console quick");
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

        public void UpdateBoltVersion()
        {
            string version = BoltVersion;
            string buildNumber = TryGetBuildNumber();
            if (!string.IsNullOrEmpty(buildNumber))
            {
                version += "-" + buildNumber;
                Console.WriteLine("Build number {0} detected. Version of nuget packages will be: {1}.", buildNumber, version);
            }

            var projects = GetPaths("src/**/*.csproj");
            foreach (var project in projects)
            {
                var regex = @"(.*""version""\s*:\s*"")(.*)("".*)";
                var content = System.IO.File.ReadAllText(project);
                bool replaced = false;
                content = Regex.Replace(content, regex, m =>
                {
                    if (replaced)
                    {
                        return m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value;
                    }
                    else
                    {
                        // replace only first occurence
                        replaced = true;
                        return m.Groups[1].Value + version + m.Groups[3].Value;
                    }
                }, RegexOptions.Multiline);

                File.WriteAllText(project, content);
                Console.WriteLine("Project {0} version updated to {1}", project, version);
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