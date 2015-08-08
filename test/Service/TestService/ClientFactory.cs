using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Client.Filters;
using Bolt.Client.Proxy;
using TestService.Core;

namespace TestService
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration()
        {
            ProxyFactory = new DynamicProxyFactory()
        };

        public static ITestContract CreateIISBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.IISBoltServer);
        }

        public static ITestContract CreateBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.BoltServer);
        }

        public static ITestContract CreateDynamicBolt()
        {
            return Config.CreateProxy<ITestContract>(Servers.BoltServer);
        }

        public static ITestContract CreateWcf()
        {
            ChannelFactory<ITestContract> respository = new ChannelFactory<ITestContract>(new BasicHttpBinding());
            ITestContract channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }

        private class LoggingFilter : IClientExecutionFilter
        {
            public async Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
            {
                Console.WriteLine($"Begin execution of {GetType().Name}");
                await next(context);
                Console.WriteLine($"Execution of {GetType().Name} finished.");
            }
        }

        private class DelayFilter : IClientExecutionFilter
        {
            public async Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
            {
                Console.WriteLine($"Begin execution of {GetType().Name}");
                await next(context);
                Console.WriteLine($"Execution of {GetType().Name} finished.");
            }
        }

        private class ErrorFilter : IClientExecutionFilter
        {
            public async Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
            {
                await next(context);
            }
        }

        private class NewlineFilter : IClientExecutionFilter
        {
            public async Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
            {
                Console.WriteLine($"Begin execution of {GetType().Name}");
                await next(context);
                Console.WriteLine($"Execution of {GetType().Name} finished.");
            }
        }
    }
}
