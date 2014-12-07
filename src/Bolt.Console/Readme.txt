You have installed tool for Bolt Client and Server side code generation. 

There are couple of steps required to start using the Bolt framework.

If you require detailed control of output code:

1. Add 'Configuration.json' file to project that contains service interfaces.  
   To generate the 'Configuration.json' file from existing assembly use 'Bolt.exe -g=<AssemblyPath>' command that 
   will generate configuration for all interfaces in the assembly.
2. Define post build step in target project. In most cases the script will look like:

	cd "$(SolutionDir)\packages\bolt.tool*\tools"
	Bolt.exe -root=$(TargetDir) -fromConfig=$(ProjectDir)Configuration.json

If default generated code is sufficient:

1. Define post build step in target project. In most cases the script will look like:

	cd "$(SolutionDir)\packages\bolt.tool*\tools"
	Bolt.exe -output=$(ProjectDir) -fromAssembly=$(TargetPath)
	
After you build the project the client && server side code will be generated. You can link these files
from your client and server projects to start using the Bolt framework.

Server:

To register Bolt and start using Bolt for sample interface "ITestContract" use code similar to:

    public void Configuration(IAppBuilder app)
    {
        app.UseBolt(new ServerConfiguration(new ProtocolBufferSerializer(), new JsonExceptionSerializer(new JsonSerializer())));
        app.UseTestContract<TestContractImplementation>();
    }

Client:

To create proxy to your server use code:

	var config = new ClientConfiguration(new ProtocolBufferSerializer(), new JsonExceptionSerializer(new JsonSerializer()), new DefaultWebRequestHandlerEx());
	var proxy = config.CreateProxy<TestContractProxy>("<ServerUrl>")
