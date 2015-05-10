@ECHO OFF

call dnu pack src\Bolt.Core\project.json --configuration Release
call dnu pack src\Bolt.Generators\project.json --configuration Release
call dnu pack src\Bolt.Console\project.json --configuration Release
call dnu pack src\Bolt.Client\project.json --configuration Release
call dnu pack src\Bolt.Server\project.json --configuration Release