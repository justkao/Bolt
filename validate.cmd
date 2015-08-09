@ECHO OFF

call restore.cmd
if %errorlevel% neq 0 exit /b %errorlevel%
call build.cmd
if %errorlevel% neq 0 exit /b %errorlevel%
call pack.cmd
if %errorlevel% neq 0 exit /b %errorlevel%
call test.cmd
if %errorlevel% neq 0 exit /b %errorlevel%


