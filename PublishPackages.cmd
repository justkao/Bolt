@ECHO OFF
 
set version="0.9.1.1"


.nuget\nuget.exe push bolt_packages\Bolt.Core.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Server.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Tool.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Client.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Client.Proxy.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Helpers.%version%.nupkg

.nuget\nuget.exe push bolt_packages\Bolt.Generators.%version%.nupkg