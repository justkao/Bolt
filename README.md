##Bolt
Lean and lightweight http communication library based on ASP.NET 5. The main focus of project 
was to create multiplatform, simple and powerfull replacement for WCF library.

####Service Contract
Bolt requires interface describing your service that will be used for communication.

```c#
public interface IFooService
{
    void DoYourThing();
}
```

####Client
* Add *Bolt.Client.Proxy* package to project (`Install-Package Bolt.Client.Proxy -pre`)
* Create proxy to your service and call remote method
```c#
var configuration = new ClientConfiguration();
configuration.ProxyFactory = new DynamicProxyFactory();
var proxy = configuration.CreateProxy<FooServiceProxy>(<service url>);
proxy.DoYourThing();
```

####Server
* Add *Bolt.Server* package to project (`Install-Package Bolt.Server -pre`)
* In you startup class use Bolt extensions to register Bolt into the pipeline

```c#
public void Configuration(IApplicationBuilder app)
{
    appBuilder.UseBolt((h) =>
    {
        // register our service
        h.Use<IFooService, FooService>();
    });
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

#### Bolt Packages
* **[Bolt.Core](https://www.nuget.org/packages/Bolt.Core/)** - contains common interfaces and helpers shared by both client and server. Multiple platforms are supported for this package. Portable Class Library(PCL) 
* **[Bolt.Client](https://www.nuget.org/packages/Bolt.Client/)** - contains client side code required to communicate with Bolt service.(PCL)
* **[Bolt.Client.Proxy](https://www.nuget.org/packages/Bolt.Client/)** - factory for dynamic proxy generation
* **[Bolt.Server](https://www.nuget.org/packages/Bolt.Server/)** - server side code required to integrate Bolt into ASP.NET 5
* **[Bolt.Generators](https://www.nuget.org/packages/Bolt.Generators/)** - classes and helpers used by Bolt.Console for code generation. Reference it if you need to generate async proxies and interfaces.(PCL)
* **[Bolt.Console](https://www.nuget.org/packages/Bolt.Tool/)** - tool used to generate Bolt proxies.

To find out more just take a look at Bolt code or check out the [Bolt.Samples](https://github.com/justkao/Bolt.Samples)
repository.

Any ideas, improvements or code contributions are welcome. Happy coding ;)
