using System;
using Bolt.Server.InstanceProviders;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static IContractInvoker UseStateLess<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            return bolt.Use<TContract>(new InstanceProvider<TContractImplementation>(), configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            BoltFramework.ValidateContract(typeof (TContract));

            return bolt.UseStateFull<TContract, TContractImplementation>(
                new MemorySessionFactory(options ?? bolt.Configuration.Options),
                configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            ISessionFactory sessionFactory,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            return
                bolt.Use<TContract>(
                    new StateFullInstanceProvider<TContractImplementation>(sessionFactory), configure);
        }

        public static IContractInvoker Use<TContract>(
            this IBoltRouteHandler bolt,
            IInstanceProvider instanceProvider,
            Action<IContractInvoker> configure = null)
        {
            if (bolt == null)
            {
                throw new ArgumentNullException(nameof(bolt));
            }


            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            var factory = bolt.ApplicationServices.GetRequiredService<IContractInvokerFactory>();

            IContractInvoker invoker = factory.Create(typeof (TContract), instanceProvider);
            invoker.Configuration.Merge(bolt.Configuration);
            configure?.Invoke(invoker);
            bolt.Add(invoker);

            return invoker;
        }
    }
}