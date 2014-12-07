@ECHO OFF
 
if not exist bolt_packages mkdir bolt_packages

.nuget\nuget.exe pack "src\Bolt.Core\Bolt.Core.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -Build -Properties Configuration=Release

.nuget\nuget.exe pack "src\Bolt.Server\Bolt.Server.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

.nuget\nuget.exe pack "src\Bolt.Client\Bolt.Client.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

.nuget\nuget.exe pack "src\Bolt.Helpers\Bolt.Helpers.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

.nuget\nuget.exe pack "src\Bolt.Generators\Bolt.Generators.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -IncludeReferencedProjects -Build -Properties Configuration=Release

.nuget\nuget.exe pack "src\Bolt.Console\Bolt.Console.csproj" -Verbosity detailed -OutputDirectory bolt_packages -Symbols -Tool -Build -Properties Configuration=Release