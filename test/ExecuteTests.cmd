@ECHO OFF

call dnx Automatic\Bolt.Core.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%

call dnx Automatic\Bolt.Client.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%

call dnx Automatic\Bolt.Server.Test\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%

call dnx Integration\Bolt.Server.IntegrationTest\project.json test
if %errorlevel% neq 0 exit /b %errorlevel%