@ECHO OFF

rem tests

call dnx -p "test\Automatic\Bolt.Core.Test" test
if %errorlevel% neq 0 goto failure

call dnx -p "test\Automatic\Bolt.Client.Test" test
if %errorlevel% neq 0 goto failure

call dnx -p "test\Automatic\Bolt.Client.Proxy.Test" test
if %errorlevel% neq 0 goto failure

call dnx -p "test\Automatic\Bolt.Server.Test" test
if %errorlevel% neq 0 goto failure

call dnx -p "test\Integration\Bolt.Server.IntegrationTest" test
if %errorlevel% neq 0 goto failure

goto success

:failure
echo Tests Failed !!!!
pause
goto end

:success
echo Tests Suceeeded !!!!
goto end

:end
exit /B %errorlevel%


