using System;
using System.Collections.Generic;
using Bolt.Client;
using System.ServiceModel;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration();

        public static ITestContract CreateIISBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.IISBoltServer);
        }

        public static ITestContract CreateBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.BoltServer);
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
