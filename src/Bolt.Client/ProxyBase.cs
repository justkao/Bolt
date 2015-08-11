using System;
using System.Reflection;
using System.Threading.Tasks;

using Bolt.Client.Pipeline;
using Bolt.Pipeline;

namespace Bolt.Client
{
    public abstract class ProxyBase : IProxy, IPipelineCallback
    {
        protected ProxyBase()
        {
        }

        protected ProxyBase(Type contract, IPipeline<ClientActionContext> pipeline)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            Contract = contract;
            Pipeline = pipeline;
        }

        protected ProxyBase(ProxyBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            Contract = proxy.Contract;
            Pipeline = proxy.Pipeline;
            State = proxy.State;
        }

        public Type Contract { get; protected set; }

        public ProxyState State { get; private set; }

        protected IPipeline<ClientActionContext> Pipeline { get; set; }

        public async Task OpenAsync()
        {
            using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.InitSessionAction, new object[] { null }))
            {
                await Pipeline.Instance(ctxt);
                State = ProxyState.Open;
            }
        }

        public async Task CloseAsync()
        {
            if (State == ProxyState.Open)
            {
                using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.DestroySessionAction, new object[] { null }))
                {
                    await Pipeline.Instance(ctxt);
                    State = ProxyState.Closed;
                }
            }
            else
            {
                State = ProxyState.Closed;
            }

            Dispose();
        }

        public async Task<object> SendAsync(MethodInfo action, params object[] parameters)
        {
            using (ClientActionContext ctxt = new ClientActionContext(this, Contract, action, parameters))
            {
                await Pipeline.Instance(ctxt);
                return ctxt.ActionResult;
            }
        }

        public void Dispose()
        {
            if (State == ProxyState.Open)
            {
                this.Close();
            }

            Pipeline?.Dispose();
            Pipeline = null;
        }

        void IPipelineCallback.ChangeState(ProxyState newState)
        {
            State = newState;
        }
    }
}
