#reference "System.Net.dll"
#reference "System.Net.Http.dll"

using System.IO;
using System.Text.RegularExpressions;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var version = Argument<string>("boltVersion", "0.40.0-alpha1");
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
    var settings = new DotNetCoreRestoreSettings
    {
        Source = nugetFeed
    };
                
    DotNetCoreRestore(settings);
});


Task("Clean")
    .Does(() =>
{
    CleanDirectory("./artifacts/");
});

Task("GenerateInterfaces")
    .Does(() =>
{      
    // DotNetCoreRun("./src/Bolt.Tools/", @"code --input ./test/Integration/");  
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("GenerateInterfaces")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbose = true
    };
    
    DotNetCoreBuild("./src/**/project.json", settings);
    DotNetCoreBuild("./test/**/project.json", settings);
    DotNetCoreBuild("./samples/**/project.json", settings);    
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
        // DotNetCoreTest(project.FullPath);
    }
    
    Information("Running integration tests");
    // DotNetCoreTest("test/integration/Bolt.Server.IntegrationTest");
});

Task("RunSample")
    .Does(() =>
{    
    DotNetCoreRun("./samples/Bolt.Sample.ContentProtection");
});

Task("RunQuickPerformance")
    .Does(() =>
{    
    var serverProcess = StartAndReturnProcess("dotnet", new ProcessSettings{ WorkingDirectory = "./test/Performance/Bolt.Performance.Server", Arguments = "run run" });
    System.Threading.Thread.Sleep(500);
    var clientReturnCode = StartProcess("dotnet", new ProcessSettings{ WorkingDirectory = "./test/Performance/Bolt.Performance.Console", Arguments = "run quick" });
    try
    {
        serverProcess.Kill();
    }
    catch(Exception e)
    {
        // OK
    }

    if (clientReturnCode != 0)
    {
        throw new Exception("Failed to run quick performance tests.");
    } 
});


Task("BuildBoltPackages")
	// .IsDependentOn("Test")
    // .IsDependentOn("RunSample")
    .IsDependentOn("RunQuickPerformance")
    .IsDependentOn("UpdateBoltVersion")
    .Does(() =>
{    
    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration
    };
    
    DotNetCorePack("./src/Bolt.Core", settings);
    DotNetCorePack("./src/Bolt.Server", settings);
    DotNetCorePack("./src/Bolt.Client", settings);
    DotNetCorePack("./src/Bolt.Tools", settings);
    
	CleanDirectory(string.Format("./artifacts/{0}", configuration));
	CopyFiles(string.Format("./src/**/*{0}*.nupkg", version), string.Format("./artifacts/{0}", configuration));
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