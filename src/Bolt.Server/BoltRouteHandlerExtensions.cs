using System;
using System.Reflection;

using Bolt.Server.InstanceProviders;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static IContractInvoker UseStateLess<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            Action<IContractInvoker> configure = null) where TContractImplementation : new()
        {
            return bolt.Use<TContract>(new InstanceProvider<TContractImplementation>(), configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : new()
        {
            Bolt.ValidateStatefullContract(typeof(TContract));

            return bolt.UseStateFull<TContract, TContractImplementation>(
                Bolt.GetInitSessionMethod(typeof(TContract)),
                Bolt.GetCloseSessionMethod(typeof(TContract)),
                new MemorySessionFactory(options ?? bolt.Configuration.Options),
                configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            MethodInfo init,
            MethodInfo release,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : new()
        {
            return bolt.UseStateFull<TContract, TContractImplementation>(
                init,
                release,
                new MemorySessionFactory(options ?? bolt.Configuration.Options),
                configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            ISessionFactory sessionFactory,
            Action<IContractInvoker> configure = null) where TContractImplementation : new()
        {
            Bolt.ValidateStatefullContract(typeof(TContract));

            return
                bolt.Use<TContract>(
                    new StateFullInstanceProvider<TContractImplementation>(
                        Bolt.GetInitSessionMethod(typeof(TContract)),
                        Bolt.GetCloseSessionMethod(typeof(TContract)),
                        sessionFactory),
                    configure);
        }

        public static IContractInvoker UseStateFull<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            MethodInfo init,
            MethodInfo release,
            ISessionFactory sessionFactory,
            Action<IContractInvoker> configure = null) where TContractImplementation : new()
        {
            return bolt.Use<TContract>(new StateFullInstanceProvider<TContractImplementation>(init, release, sessionFactory), configure);
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

            IContractInvoker invoker = factory.Create(typeof(TContract), instanceProvider);
            invoker.Configuration.Merge(bolt.Configuration);
            configure?.Invoke(invoker);
            bolt.Add(invoker);

            return invoker;
        }
    }
}