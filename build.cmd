@ECHO OFF

call dnu build src\Bolt.Core\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build src\Bolt.Generators\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build src\Bolt.Console\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build src\Bolt.Client\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build src\Bolt.Server\project.json --configuration Release
if %errorlevel% neq 0 goto failure

rem tests
call dnu build test\Automatic\Bolt.Core.Test\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Automatic\Bolt.Client.Test\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Automatic\Bolt.Core.Test\project.json --configuration Release
if %errorlevel% neq 0 goto failure

call dnu build test\Integration\Bolt.Server.IntegrationTest\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Integration\Bolt.Server.IntegrationTest.Core\project.json --configuration Release
if %errorlevel% neq 0 goto failure

call dnu build test\Performance\Bolt.Performance.Core\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Performance\Bolt.Performance.Console\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Performance\Bolt.Performance.Server\project.json --configuration Release
if %errorlevel% neq 0 goto failure
call dnu build test\Performance\Bolt.Performance.Server.Wcf\project.json --configuration Release
if %errorlevel% neq 0 goto failure

goto success

:failure
echo Build Failed !!!!
pause
goto end

:success
echo Build Suceeeded !!!!
goto end

:end
exit /B %errorlevel%
