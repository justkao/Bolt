@ECHO OFF

call dnu restore src\Bolt.Core\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore src\Bolt.Generators\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore src\Bolt.Console\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore src\Bolt.Client\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore src\Bolt.Client.Proxy\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore src\Bolt.Server\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%

rem tests
call dnu restore test\Automatic\Bolt.Core.Test\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Automatic\Bolt.Client.Test\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Automatic\Bolt.Core.Test\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Automatic\Bolt.Client.Proxy.Test\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%

call dnu restore test\Integration\Bolt.Server.IntegrationTest\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Integration\Bolt.Server.IntegrationTest.Core\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%

call dnu restore test\Service\TestService.Core\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Service\TestService\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Service\TestService.Server.Bolt\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%
call dnu restore test\Service\TestService.Server.Wcf\project.json --parallel
if %errorlevel% neq 0 exit /b %errorlevel%