using System;
using Bolt.Server.InstanceProviders;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static TInvoker UseStateLess<TInvoker, TContractImplementation>(this IBoltRouteHandler bolt, Action<TInvoker> configure = null)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return bolt.Use(new InstanceProvider<TContractImplementation>(), configure);
        }

        public static TInvoker UseStateFull<TInvoker, TContractImplementation>(this IBoltRouteHandler bolt, ActionDescriptor init, ActionDescriptor release, BoltServerOptions options = null, Action<TInvoker> configure = null)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return bolt.Use(new StateFullInstanceProvider<TContractImplementation>(init, release, options ?? bolt.Options), configure);
        }

        public static TInvoker Use<TInvoker>(this IBoltRouteHandler bolt, IInstanceProvider instanceProvider, Action<TInvoker> configure = null)
            where TInvoker : ContractInvoker, new()
        {
            if (bolt == null)
            {
                throw new ArgumentNullException(nameof(bolt));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            TInvoker contractInvoker = new TInvoker();
            contractInvoker.Init(bolt, instanceProvider);
            bolt.Add(contractInvoker);
            configure?.Invoke(contractInvoker);

            return contractInvoker;
        }
    }
}