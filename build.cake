#reference "System.Net.dll"
#reference "System.Net.Http.dll"

using System.IO;
using System.Text.RegularExpressions;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var version = Argument<string>("boltVersion", "0.22.0-alpha1");
var nugetFeed = "https://api.nuget.org/v3/index.json";

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
var buildNumber = AppVeyor.Environment.Build.Number;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(() =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Restore")
    .Does(() =>
{
    var settings = new DNURestoreSettings
    {
        Parallel = true,
        Locked = DNULocked.Lock,
        Sources = new [] { nugetFeed },
        Quiet = true
    };
                
    DNURestore(settings);
});


Task("Clean")
    .Does(() =>
{
    CleanDirectory("./artifacts/");
});

Task("GenerateInterfaces")
    .Does(() =>
{      
    var exitCode = StartProcess("dnx", new ProcessSettings{ WorkingDirectory = "./Test/Integration/Bolt.Server.IntegrationTest.Core", Arguments = "gen" });
    if (exitCode != 0)
    {
        throw new Exception("Failed to generate Bolt Proxy for integration tests.");
    }
    
    exitCode = StartProcess("dnx", new ProcessSettings{ WorkingDirectory = "./Test/Performance/Bolt.Performance.Core", Arguments = "gen" });
    if (exitCode != 0)
    {
        throw new Exception("Failed to generate Bolt Proxy for performance tests.");
    }
    
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("GenerateInterfaces")
    .Does(() =>
{
    var settings = new DNUBuildSettings
    {
        Configurations = new[] { configuration },
        Quiet = true
    };
    
    DNUBuild("./src/*", settings);
    DNUBuild("./samples/*", settings);
    DNUBuild("./test/Common/*", settings);
    DNUBuild("./test/Automatic/*", settings);
    DNUBuild("./test/Integration/*", settings);
    DNUBuild("./test/Performance/*", settings);    
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{   
    var testSuccess = 0;
    var testProjectRoots = GetDirectories("test/Automatic/*");
    foreach (var project in testProjectRoots)
    {
        Information("Running tests from {0}", project.GetDirectoryName());
        testSuccess = StartProcess("dnx", new ProcessSettings {
            WorkingDirectory = project,
            Arguments = "test"
        });
    
        Information("Test execution status code: {0}", testSuccess);
        if (testSuccess != 0)
        {
            throw new InvalidOperationException("Failed to execute tests for " + project);
        }
    }
    
    Information("Running integration tests");
    testSuccess = StartProcess("dnx", new ProcessSettings {
        WorkingDirectory = "test/integration/Bolt.Server.IntegrationTest",
        Arguments = "test"
    });
    
    if (testSuccess != 0)
    {
        throw new InvalidOperationException("Failed to exectute integration tests");
    }
});

Task("BuildBoltPackages")
    .IsDependentOn("Test")
    .IsDependentOn("UpdateBoltVersion")
    .Does(() =>
{    
    var settings = new DNUPackSettings
    {
        Configurations = new[] { configuration },
        OutputDirectory = "./artifacts/",
        Quiet = true
    };
    
    DNUPack("./src/*", settings);
});

Task("UpdateBoltVersion")
    .Does(() =>
{    
    if (AppVeyor.IsRunningOnAppVeyor)
    {
        version += "-" + buildNumber;
        Information("Build number {0} detected. Version of nuget packages will be: {1}.", buildNumber, version);     
    }

    var projects = GetFiles("./src/**/project.json");
    foreach(var project in projects)
    {    
        var regex = @"(.*""version""\s*:\s*"")(.*)("".*)";
        var content = System.IO.File.ReadAllText(project.FullPath);
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
        System.IO.File.WriteAllText(project.FullPath, content);
        Information("Project {0} version updated to {1}", project.FullPath, version);       
    }
});

Task("PublishBoltPackages")
    .IsDependentOn("BuildBoltPackages")
    .Does(() =>
{    
    var packages = GetFiles("./Artifacts/Release/*.nupkg").Where(f => !f.FullPath.EndsWith("symbols.nupkg"));
    NuGetPush(packages, new NuGetPushSettings());
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("BuildBoltPackages");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);