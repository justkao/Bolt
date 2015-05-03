//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.

//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bolt.Server;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Server.IntegrationTest.Core.Parameters;


namespace Bolt.Server.IntegrationTest.Core
{
    public partial class TestContractStateFullActions : Bolt.Server.ContractActions<Bolt.Server.IntegrationTest.Core.TestContractStateFullDescriptor>
    {
        // useless comment added by user generator - 'Bolt.Server.IntegrationTest.Core.UserCodeGenerator', Context - ''

        public TestContractStateFullActions()
        {
            Add(Descriptor.Init, TestContractStateFull_Init);
            Add(Descriptor.InitEx, TestContractStateFull_InitEx);
            Add(Descriptor.SetState, TestContractStateFull_SetState);
            Add(Descriptor.GetState, TestContractStateFull_GetState);
            Add(Descriptor.NextCallWillFailProxy, TestContractStateFull_NextCallWillFailProxy);
            Add(Descriptor.Destroy, TestContractStateFull_Destroy);
        }

        protected virtual Task TestContractStateFull_Init(ServerActionContext context)
        {
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            instance.Init();
            return Task.FromResult(true);
        }

        protected virtual Task TestContractStateFull_InitEx(ServerActionContext context)
        {
            var parameters = context.GetRequiredParameters<Bolt.Server.IntegrationTest.Core.Parameters.InitExParameters>();
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            instance.InitEx(parameters.FailOperation);
            return Task.FromResult(true);
        }

        protected virtual Task TestContractStateFull_SetState(ServerActionContext context)
        {
            var parameters = context.GetRequiredParameters<Bolt.Server.IntegrationTest.Core.Parameters.SetStateParameters>();
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            instance.SetState(parameters.State);
            return Task.FromResult(true);
        }

        protected virtual Task TestContractStateFull_GetState(ServerActionContext context)
        {
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            context.Result = instance.GetState();
            return Task.FromResult(true);
        }

        protected virtual Task TestContractStateFull_NextCallWillFailProxy(ServerActionContext context)
        {
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            instance.NextCallWillFailProxy();
            return Task.FromResult(true);
        }

        protected virtual Task TestContractStateFull_Destroy(ServerActionContext context)
        {
            var instance = context.GetRequiredInstance<ITestContractStateFull>();
            instance.Destroy();
            return Task.FromResult(true);
        }
    }
}

namespace Bolt.Server
{
    public static partial class TestContractStateFullActionsExtensions
    {
        public static IContractInvoker UseTestContractStateFull(this IBoltRouteHandler bolt, Bolt.Server.IntegrationTest.Core.ITestContractStateFull instance)
        {
            return bolt.UseTestContractStateFull(new StaticInstanceProvider(instance));
        }

        public static IContractInvoker UseTestContractStateFull<TImplementation>(this IBoltRouteHandler bolt) where TImplementation: Bolt.Server.IntegrationTest.Core.ITestContractStateFull
        {
            return bolt.UseTestContractStateFull(new InstanceProvider<TImplementation>());
        }

        public static IContractInvoker UseStateFullTestContractStateFull<TImplementation>(this IBoltRouteHandler bolt, Bolt.Server.BoltServerOptions options = null) where TImplementation: Bolt.Server.IntegrationTest.Core.ITestContractStateFull
        {
            var initSessionAction = TestContractStateFullDescriptor.Default.Init;
            var closeSessionAction = TestContractStateFullDescriptor.Default.Destroy;
            return bolt.UseTestContractStateFull(new StateFullInstanceProvider<TImplementation>(initSessionAction, closeSessionAction, options ?? bolt.Configuration.Options));
        }

        public static IContractInvoker UseStateFullTestContractStateFull<TImplementation>(this IBoltRouteHandler bolt, ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, Bolt.Server.BoltServerOptions options = null) where TImplementation: Bolt.Server.IntegrationTest.Core.ITestContractStateFull
        {
            return bolt.UseTestContractStateFull(new StateFullInstanceProvider<TImplementation>(initInstanceAction, releaseInstanceAction, options ?? bolt.Configuration.Options));
        }

        public static IContractInvoker UseTestContractStateFull(this IBoltRouteHandler bolt, IInstanceProvider instanceProvider)
        {
            return bolt.Use(new Bolt.Server.IntegrationTest.Core.TestContractStateFullActions(), instanceProvider);
        }
    }
}