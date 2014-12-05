@ECHO OFF

nuget.exe pack "..\src\Bolt.Server\Bolt.Server.csproj" -IncludeReferencedProjects

nuget.exe pack "..\src\Bolt.Client\Bolt.Client.csproj" -IncludeReferencedProjects

nuget.exe pack "..\src\Bolt.Helpers\Bolt.Helpers.csproj" -IncludeReferencedProjects

nuget.exe pack "..\src\Bolt.Console\Bolt.Console.csproj"