@ECHO OFF
 
set version="0.16.0-alpha1"

if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Sources.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Core.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Server.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Console.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Client.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Client.Proxy.%version%.nupkg
if %errorlevel% neq 0 goto failure

.nuget\nuget.exe push packages\Bolt.Generators.%version%.nupkg
if %errorlevel% neq 0 goto failure

goto success

:failure
echo Publish Failed !!!!
pause
goto end

:success
echo Publish Suceeeded !!!!
goto end

:end
pause
exit /B %errorlevel%
