#reference "System.Net.dll"
#reference "System.Net.Http.dll"

using System.Net.Http;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

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
        Sources = new [] { "https://api.nuget.org/v3/index.json" },
        Quiet = true
    };
                
    var projects = GetFiles("./src/**/project.json");
    foreach(var project in projects)
    {
        DNURestore(settings);
    }
});


Task("Clean")
    .Does(() =>
{
    CleanDirectory("./artifacts/");
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var settings = new DNUBuildSettings
    {
        Configurations = new[] { configuration },
        Quiet = true
    };
    
    DNUBuild("./src/*", settings);
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

Task("Pack")
    .IsDependentOn("Test")
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

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("Pack");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);