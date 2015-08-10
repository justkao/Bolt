@ECHO OFF

call restore.cmd
if %errorlevel% neq 0 goto failure

call build.cmd
if %errorlevel% neq 0 goto failure

call pack.cmd
if %errorlevel% neq 0 goto failure

call test.cmd
if %errorlevel% neq 0 goto failure

goto success

:failure
echo Build Failed !!!!
goto end

:success
echo Build Sucess !!!!
goto end

:end
pause
exit /B %errorlevel%

