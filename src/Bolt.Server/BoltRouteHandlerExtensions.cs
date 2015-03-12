using System;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static TInvoker UseStateLess<TInvoker, TContractImplementation>(this IBoltRouteHandler bolt)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return bolt.Use<TInvoker>(new InstanceProvider<TContractImplementation>());
        }

        public static TInvoker UseStateFull<TInvoker, TContractImplementation>(this IBoltRouteHandler bolt, ActionDescriptor init, ActionDescriptor release, BoltServerOptions options = null)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return bolt.Use<TInvoker>(new StateFullInstanceProvider<TContractImplementation>(init, release, options ?? bolt.Options));
        }

        public static TInvoker Use<TInvoker>(this IBoltRouteHandler bolt, IInstanceProvider instanceProvider)
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

            return contractInvoker;
        }
    }
}