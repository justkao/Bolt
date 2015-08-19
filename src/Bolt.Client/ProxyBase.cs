using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;

namespace Bolt.Client
{
    public abstract class ProxyBase : IProxy, IPipelineCallback
    {
        private IPipeline<ClientActionContext> _pipeline;

        protected ProxyBase()
        {
        }

        protected ProxyBase(Type contract, IPipeline<ClientActionContext> pipeline)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            Contract = contract;
            _pipeline = pipeline;
        }

        protected ProxyBase(ProxyBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            Contract = proxy.Contract;
            _pipeline = proxy.Pipeline;
            State = proxy.State;
        }

        public Type Contract { get; protected set; }

        public ProxyState State { get; private set; }

        protected IPipeline<ClientActionContext> Pipeline
        {
            get
            {
                if (_pipeline == null)
                {
                    throw new ProxyClosedException();
                }

                return _pipeline;
            }

            set { _pipeline = value; }
        }

        public async Task OpenAsync()
        {
            using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.GetSessionDescriptor(Contract).InitSession, null))
            {
                await Pipeline.Instance(ctxt);
                State = ProxyState.Open;
            }
        }

        public async Task CloseAsync()
        {
            if (State == ProxyState.Open)
            {
                using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.GetSessionDescriptor(Contract).DestroySession, null))
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

            _pipeline = null;
        }

        void IPipelineCallback.ChangeState(ProxyState newState)
        {
            State = newState;
        }
    }
}
