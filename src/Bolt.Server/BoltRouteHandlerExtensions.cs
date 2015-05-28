using System;
using Bolt.Server.InstanceProviders;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
{
    public static class BoltRouteHandlerExtensions
    {
        public static IContractInvoker UseStateLess<TContractImplementation>(this IBoltRouteHandler bolt, IContractActions actions, Action<IContractInvoker> configure = null)
            where TContractImplementation : new()
        {
            return bolt.Use(actions, new InstanceProvider<TContractImplementation>(), configure);
        }

        public static IContractInvoker UseStateFull<TContractImplementation>(this IBoltRouteHandler bolt, IContractActions actions, ActionDescriptor init, ActionDescriptor release, BoltServerOptions options = null, Action<IContractInvoker> configure = null)
            where TContractImplementation : new()
        {
            return bolt.UseStateFull<TContractImplementation>(actions, init, release, new MemorySessionFactory(options ?? bolt.Configuration.Options), configure);
        }

        public static IContractInvoker UseStateFull<TContractImplementation>(this IBoltRouteHandler bolt, IContractActions actions, ActionDescriptor init, ActionDescriptor release, ISessionFactory sessionFactory, Action<IContractInvoker> configure = null)
            where TContractImplementation : new()
        {
            return bolt.Use(actions, new StateFullInstanceProvider<TContractImplementation>(init, release, sessionFactory), configure);
        }

        public static IContractInvoker Use(this IBoltRouteHandler bolt, IContractActions actions, IInstanceProvider instanceProvider, Action<IContractInvoker> configure = null)
        {
            if (bolt == null)
            {
                throw new ArgumentNullException(nameof(bolt));
            }

            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            var invoker = bolt.ApplicationServices.GetRequiredService<IContractInvoker>();
            invoker.Actions = actions;
            invoker.Configuration.Merge(bolt.Configuration);
            invoker.InstanceProvider = instanceProvider;
            invoker.Parent = bolt;
            configure?.Invoke(invoker);
            bolt.Add(invoker);

            return invoker;
        }
    }
}