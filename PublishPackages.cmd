@ECHO OFF
 
set version="0.13.0-alpha"

call validate.cmd
if %errorlevel% neq 0 exit /b %errorlevel%

.nuget\nuget.exe push packages\Bolt.Common.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Core.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Server.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Console.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Client.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Client.Proxy.%version%.nupkg

.nuget\nuget.exe push packages\Bolt.Generators.%version%.nupkg