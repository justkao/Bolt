@ECHO OFF

call dnu restore Automatic\Bolt.Core.Test
call dnx Automatic\Bolt.Core.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%

call dnu restore Integration\Bolt.Server.IntegrationTest
call dnx Integration\Bolt.Server.IntegrationTest\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%