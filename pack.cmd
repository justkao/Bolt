@ECHO OFF

if not exist packages mkdir packages


call dnu pack src\Bolt.Common\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Common\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Core\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Core\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Generators\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Generators\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Console\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Console\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Client\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Client\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Client.Proxy\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Client.Proxy\ %%f in (*.nupkg) do copy "%%f" packages\

call dnu pack src\Bolt.Server\project.json --configuration Release
if %errorlevel% neq 0 goto failure
for /R src\Bolt.Server\ %%f in (*.nupkg) do copy "%%f" packages\

goto success

:failure
echo Pack Failed !!!!
pause
goto end

:success
echo Pack Suceeeded !!!!
goto end

:end
exit /B %errorlevel%

