@ECHO OFF

nuget.exe pack "..\src\Bolt.Core\Bolt.Core.csproj"

nuget.exe pack "..\src\Bolt.Server\Bolt.Server.csproj"

nuget.exe pack "..\src\Bolt.Client\Bolt.Client.csproj"

nuget.exe pack "..\src\Bolt.Helpers\Bolt.Helpers.csproj"

nuget.exe pack "..\src\Bolt.Console\Bolt.Console.csproj"