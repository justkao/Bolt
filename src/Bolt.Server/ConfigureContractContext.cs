using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server
{
    public class ConfigureContractContext
    {
        private readonly List<IMiddleware<ServerActionContext>> _middlewares = new List<IMiddleware<ServerActionContext>>();
        private readonly IServiceProvider _applicationServices;

        public ConfigureContractContext(IContractInvoker contractInvoker, IServiceProvider applicationServices)
        {
            ContractInvoker = contractInvoker ?? throw new ArgumentNullException(nameof(contractInvoker));
            _applicationServices = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
        }

        public IContractInvoker ContractInvoker { get; }

        public ConfigureContractContext Use(Func<ActionDelegate<ServerActionContext>, ServerActionContext, Task> handler) 
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _middlewares.Add(new DelegatedMiddleware<ServerActionContext>(handler));
            return this;
        }

        public ConfigureContractContext Use<T>() where T : IMiddleware<ServerActionContext>
        {
            _middlewares.Add(ActivatorUtilities.GetServiceOrCreateInstance<T>(_applicationServices));
            return this;
        }

        public IEnumerable<IMiddleware<ServerActionContext>> Middlewares => _middlewares;
    }
}