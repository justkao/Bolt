## Bolt

[![Build status](https://ci.appveyor.com/api/projects/status/b97hsk15u6cw000m/branch/master?svg=true)](https://ci.appveyor.com/project/justkao/bolt)

Lean and lightweight http communication library based on ASP.Net Core. The main focus of project 
was to create multiplatform, simple and powerfull replacement for WCF library.

#### Service Contract
Bolt requires interface describing your service that will be used for communication. No annotations are 
required.

```c#
public interface IFooService
{
    // timeout and cancellation support
    [Timeout(4500)]   
    Task<string> GetDataAsync(CancellationToken token);
    
    Task SendDataAsync(string param1, int param2);
}
```

#### Client
* Add *Bolt.Client* package to project (`Install-Package Bolt.Client -pre`)
* Create proxy to your service and call remote method
```c#
var configuration = new ClientConfiguration();
IFooService proxy = configuration.CreateProxy<IFooService>(<service url>);
await proxy.GetDataAsync(CancellationToken.None);
```

#### Server
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
* Recoverable proxies
* Server failover support
* Streaming
* Modularity - every component and behavior of Bolt is replaceable
* Generation of synchronous or asynchronous interfaces using the dotnet-bolt tool

#### Bolt Packages
* **[Bolt.Core](https://www.nuget.org/packages/Bolt.Core/)** - contains common interfaces and helpers shared by both client and server.
* **[Bolt.Client](https://www.nuget.org/packages/Bolt.Client/)** - contains client side code required to communicate with Bolt service.
* **[Bolt.Server](https://www.nuget.org/packages/Bolt.Server/)** - server side code required to integrate Bolt into ASP.NET Core
* **[dotnet-bolt](https://www.nuget.org/packages/dotnet-bolt/)** - tool used to generate synchronous and asynchronous interfaces.

To find out more just take a look at Bolt code or check out the [Bolt Samples](https://github.com/justkao/Bolt/tree/master/samples)
source code.

Any ideas, improvements or code contributions are welcome. Happy coding ;)
