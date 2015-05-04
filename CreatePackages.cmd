@ECHO OFF
 
if not exist bolt_packages mkdir bolt_packages

call dnu pack src\Bolt.Core\project.json --out bolt_packages\Bolt.Core --configuration Release --quiet
call dnu pack src\Bolt.Generators\project.json --out bolt_packages\Bolt.Generators --configuration Release --quiet
call dnu pack src\Bolt.Console\project.json --out bolt_packages\Bolt.Console --configuration Release --quiet
call dnu pack src\Bolt.Client\project.json --out bolt_packages\Bolt.Client --configuration Release --quiet
call dnu pack src\Bolt.Server\project.json --out bolt_packages\Bolt.Server --configuration Release --quiet