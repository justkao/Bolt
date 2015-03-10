using System;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static TInvoker UseStateLessContractInvoker<TInvoker, TContractImplementation>(
            this IBoltRouteHandler bolt)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return bolt.UseContractInvoker<TInvoker>(new InstanceProvider<TContractImplementation>());
        }

        public static TInvoker UseStateFullContractInvoker<TInvoker, TContractImplementation>(
            this IBoltRouteHandler bolt,
            ActionDescriptor initInstanceAction,
            ActionDescriptor releaseInstanceAction, 
            BoltServerOptions options = null)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            options = options ?? bolt.Options;

            return bolt.UseContractInvoker<TInvoker>(
                new StateFullInstanceProvider<TContractImplementation>(
                    initInstanceAction,
                    releaseInstanceAction, options));
        }

        public static TInvoker UseContractInvoker<TInvoker>(
            this IBoltRouteHandler bolt,
            IInstanceProvider instanceProvider)
            where TInvoker : ContractInvoker, new()
        {
            if ( bolt == null)
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