@ECHO OFF

call dnu restore src\Bolt.Core\project.json --parallel
call dnu restore src\Bolt.Generators\project.json --parallel
call dnu restore src\Bolt.Console\project.json --parallel
call dnu restore src\Bolt.Client\project.json --parallel
call dnu restore src\Bolt.Server\project.json --parallel

rem tests
call dnu restore test\automatic\Bolt.Core.Test\project.json --parallel
call dnu restore test\automatic\Bolt.Client.Test\project.json --parallel
call dnu restore test\automatic\Bolt.Core.Test\project.json --parallel

call dnu restore test\Integration\Bolt.Server.IntegrationTest\project.json --parallel
call dnu restore test\Integration\Bolt.Server.IntegrationTest.Core\project.json --parallel

call dnu restore test\Service\TestService.Core\project.json --parallel
call dnu restore test\Service\TestService\project.json --parallel
call dnu restore test\Service\TestService.Server.Bolt\project.json --parallel
call dnu restore test\Service\TestService.Server.Wcf\project.json --parallel
