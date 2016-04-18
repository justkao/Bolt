##Bolt

[![Build status](https://ci.appveyor.com/api/projects/status/b97hsk15u6cw000m/branch/master?svg=true)](https://ci.appveyor.com/project/justkao/bolt)

Lean and lightweight http communication library based on ASP.Net Core. The main focus of project 
was to create multiplatform, simple and powerfull replacement for WCF library.

####Service Contract
Bolt requires interface describing your service that will be used for communication.

```c#
public interface IFooService
{
    void SimpleMethod();
    
    // Support for asynchrony, action timeouts and cancellation tokens
    [Timeout(4500)]
    Task SimpleMethodWitCancellationAsync(CancellationToken token);
}
```

####Client
* Add *Bolt.Client* package to project (`Install-Package Bolt.Client -pre`)
* Create proxy to your service and call remote method
```c#
var configuration = new ClientConfiguration();
IFooService proxy = configuration.CreateProxy<IFooService>(<service url>);
proxy.DoYourThing();
```

####Server
* Add *Bolt.Server* package to project (`Install-Package Bolt.Server -pre`)
* In you startup class use Bolt extensions to register Bolt into the pipeline

```c#

public void ConfigureServices(IServiceCollection services)
{
    services.AddLogging();
    services.AddOptions();
    services.AddBolt();
}

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
* CancellationToken support
* Asynchronous methods
* Recoverable Proxy
* Server Failover support
* Modularity - every component and behavior of Bolt is replaceable
* User Code Generators - plug your own code into the generated proxies
* Configuration Based Generation - define Configuration.json file to describe how proxies should be generated

#### Bolt Packages
* **[Bolt.Core](https://www.nuget.org/packages/Bolt.Core/)** - contains common interfaces and helpers shared by both client and server.
* **[Bolt.Client](https://www.nuget.org/packages/Bolt.Client/)** - contains client side code required to communicate with Bolt service.
* **[Bolt.Server](https://www.nuget.org/packages/Bolt.Server/)** - server side code required to integrate Bolt into ASP.NET Core
* **[Bolt.Console](https://www.nuget.org/packages/Bolt.Tool/)** - tool used to generate syncronous and asynchronous interfaces.

To find out more just take a look at Bolt code or check out the [Bolt.Samples](https://github.com/justkao/Bolt.Samples)
repository.

Any ideas, improvements or code contributions are welcome. Happy coding ;)
