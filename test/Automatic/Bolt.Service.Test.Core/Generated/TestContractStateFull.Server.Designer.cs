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
using Bolt.Service.Test.Core;
using Bolt.Service.Test.Core.Parameters;
using Owin;


namespace Bolt.Service.Test.Core
{
    public partial class TestContractStateFullInvoker : Bolt.Server.ContractInvoker<Bolt.Service.Test.Core.TestContractStateFullDescriptor>
    {
        public override void Init()
        {
            if (Descriptor == null)
            {
                Descriptor = Bolt.Service.Test.Core.TestContractStateFullDescriptor.Default;
            }

            AddAction(Descriptor.Init, TestContractStateFull_Init);
            AddAction(Descriptor.SetState, TestContractStateFull_SetState);
            AddAction(Descriptor.GetState, TestContractStateFull_GetState);
            AddAction(Descriptor.Destroy, TestContractStateFull_Destroy);

            base.Init();
        }

        public virtual Bolt.Service.Test.Core.TestContractStateFullDescriptor ContractDescriptor { get; set; }

        protected virtual async Task TestContractStateFull_Init(Bolt.Server.ServerExecutionContext context)
        {
            var instance = InstanceProvider.GetInstance<ITestContractStateFull>(context);
            instance.Init();
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContractStateFull_SetState(Bolt.Server.ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<SetStateParameters>(context);
            var instance = InstanceProvider.GetInstance<ITestContractStateFull>(context);
            instance.SetState(parameters.State);
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContractStateFull_GetState(Bolt.Server.ServerExecutionContext context)
        {
            var instance = InstanceProvider.GetInstance<ITestContractStateFull>(context);
            var result = instance.GetState();
            await ResponseHandler.Handle(context, result);
        }

        protected virtual async Task TestContractStateFull_Destroy(Bolt.Server.ServerExecutionContext context)
        {
            var instance = InstanceProvider.GetInstance<ITestContractStateFull>(context);
            instance.Destroy();
            await ResponseHandler.Handle(context);
        }
    }
}

namespace Bolt.Server
{
    public static partial class TestContractStateFullInvokerExtensions
    {
        public static IAppBuilder UseTestContractStateFull(this IAppBuilder app, Bolt.Service.Test.Core.ITestContractStateFull instance)
        {
            return app.UseTestContractStateFull(new StaticInstanceProvider(instance));
        }

        public static IAppBuilder UseTestContractStateFull<TImplementation>(this IAppBuilder app) where TImplementation: Bolt.Service.Test.Core.ITestContractStateFull, new()
        {
            return app.UseTestContractStateFull(new InstanceProvider<TImplementation>());
        }

        public static IAppBuilder UseStateFullTestContractStateFull<TImplementation>(this IAppBuilder app, string sessionHeader = null, TimeSpan? sessionTimeout = null) where TImplementation: Bolt.Service.Test.Core.ITestContractStateFull, new()
        {
            return app.UseTestContractStateFull(new StateFullInstanceProvider<TImplementation>(sessionHeader ?? app.GetBolt().Configuration.SessionHeader, sessionTimeout ?? app.GetBolt().Configuration.StateFullInstanceLifetime));
        }

        public static IAppBuilder UseTestContractStateFull(this IAppBuilder app, IInstanceProvider instanceProvider)
        {
            var boltExecutor = app.GetBolt();
            var invoker = new Bolt.Service.Test.Core.TestContractStateFullInvoker();
            invoker.Descriptor = Bolt.Service.Test.Core.TestContractStateFullDescriptor.Default;
            invoker.Init(boltExecutor.Configuration);
            invoker.InstanceProvider = instanceProvider;
            boltExecutor.Add(invoker);

            return app;
        }

    }
}

