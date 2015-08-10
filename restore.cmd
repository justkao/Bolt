@ECHO OFF

call dnu restore src --parallel
if %errorlevel% neq 0 goto failure

call dnu restore test --parallel
if %errorlevel% neq 0 goto failure

goto success

:failure
echo Restore Failed !!!!
pause
goto end

:success
echo Restore Suceeeded !!!!
goto end

:end
exit /B %errorlevel%
