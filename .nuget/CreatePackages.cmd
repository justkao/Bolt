@ECHO OFF
 
nuget.exe pack "..\src\Bolt.Core\Bolt.Core.csproj" -Symbols -Build -Properties Configuration=Release

nuget.exe pack "..\src\Bolt.Server\Bolt.Server.csproj" -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

nuget.exe pack "..\src\Bolt.Client\Bolt.Client.csproj" -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

nuget.exe pack "..\src\Bolt.Helpers\Bolt.Helpers.csproj" -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

nuget.exe pack "..\src\Bolt.Console\Bolt.Console.csproj" -Symbols -Tool -Build -Properties Configuration=Release