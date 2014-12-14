##Bolt
Lean and lightweight http communication library based on Katana project. The main focus of project 
was to create multiplatform, simple and powerfull replacement for WCF library.

####Service Contract
Bolt requires interface describing your service that will be used to generate client and server side code. 
This interface should be included in separate project.

```
    public interface IFooService
    {
        void DoYourThing();
    }
```
Add *Bolt.Tool* package to project (`Install-Package Bolt.Tool`) and define following post build event: 
```
	cd "$(SolutionDir)\packages\bolt.tool*\tools"
	Bolt.exe -output="$(ProjectDir)\" -fromAssembly=$(TargetPath)
```
After the project is build the server and client side code will be generated. You can then link this code from other projects.

####Client
* Add *Bolt.Client* package to project (`Install-Package Bolt.Client`)
* Link generated files from contract project
* Create proxy to your service and call remote method
```
var serializer = new JsonSerializer();
var configuration = new ClientConfiguration(serializer, new JsonExceptionSerializer(serializer));
var proxy = configuration.CreateProxy<FooServiceProxy>(<service url>);
proxy.DoYourThing();
```

####Server
* Add *Bolt.Server* package to project (`Install-Package Bolt.Server`)
* Link generated files from contract project
* In you startup class use Bolt extensions to register Bolt into the pipeline

```
        public void Configuration(IAppBuilder app)
        {
            JsonSerializer serializer = new JsonSerializer();
            ServerConfiguration configuration = new ServerConfiguration(serializer, new JsonExceptionSerializer(serializer));

            // register bolt 
            app.UseBolt(configuration);

            // register our service - extension generated by Bolt tool 
            app.UseFooService<FooService>();
        }
```
Now you are ready to experiment with Bolt. This was just the very simple scenario for Bolt usage.
Bolt also supports:

* Sessions
* Generation of async interfaces
* CancellationToken 
* Multiple Content Types (json, xml, protocol buffers)
* Asynchronous methods
* Recoverable Proxy
* Server Failover support
* Modularity - every component and behavior of Bolt is replaceable
* User Code Generators - plug your own code into the generated classes
* Configuration Based Generation - define Configuration.json file to describe how contracts should be generated

To find out more just take a look at Bolt code or check out the [Bolt.Samples](https://github.com/justkao/Bolt.Samples)
repository.

Any ideas, improvements or code contributions are welcome. Happy coding ;)
