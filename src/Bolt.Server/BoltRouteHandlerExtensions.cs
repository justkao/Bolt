using System;
using Bolt.Server.InstanceProviders;
using Bolt.Server.Session;

using Microsoft.AspNet.Session;
using Microsoft.Framework.Caching.Distributed;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static IContractInvoker Use<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            return bolt.Use<TContract>(new InstanceProvider<TContractImplementation>(), configure);
        }

        public static IContractInvoker UseMemorySession<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            BoltFramework.ValidateContract(typeof (TContract));

            var factory = new MemorySessionFactory(
                options ?? bolt.Configuration.Options,
                bolt.ApplicationServices.GetRequiredService<IServerSessionHandler>());

            return bolt.UseSession<TContract, TContractImplementation>(factory, configure);
        }

        public static IContractInvoker UseDistributedSession<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            DistributedSessionFactory factory = new DistributedSessionFactory(
                options ?? bolt.Configuration.Options,
                new DistributedSessionStore(
                    bolt.ApplicationServices.GetRequiredService<IDistributedCache>(),
                    bolt.ApplicationServices.GetRequiredService<ILoggerFactory>()),
                bolt.ApplicationServices.GetRequiredService<IServerSessionHandler>());

            return bolt.UseSession<TContract, TContractImplementation>(factory, configure);
        }

        public static IContractInvoker UseDistributedSession<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            ISessionStore sessionStore,
            BoltServerOptions options = null,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            var factory = new DistributedSessionFactory(
                options ?? bolt.Configuration.Options,
                sessionStore,
                bolt.ApplicationServices.GetRequiredService<IServerSessionHandler>());

            return bolt.UseSession<TContract, TContractImplementation>(factory, configure);
        }

        public static IContractInvoker UseSession<TContract, TContractImplementation>(
            this IBoltRouteHandler bolt,
            ISessionFactory sessionFactory,
            Action<IContractInvoker> configure = null) where TContractImplementation : TContract
        {
            return bolt.Use<TContract>(new SessionInstanceProvider<TContractImplementation>(sessionFactory), configure);
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