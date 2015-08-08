@ECHO OFF

rem tests
call dnx test\Automatic\Bolt.Core.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%
call dnx test\Automatic\Bolt.Client.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%
call dnx test\Automatic\Bolt.Client.Proxy.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%
call dnx test\Automatic\Bolt.Core.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%
call dnx test\Integration\Bolt.Server.IntegrationTest\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%


