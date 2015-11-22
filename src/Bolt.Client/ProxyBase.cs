using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using System.Collections.Concurrent;

namespace Bolt.Client
{
    public abstract class ProxyBase : IProxy, IPipelineCallback
    {
        private IClientPipeline _pipeline;
        private readonly ConcurrentQueue<ClientActionContext> _contexts = new ConcurrentQueue<ClientActionContext>();
        private readonly int _poolSize = Environment.ProcessorCount * 5;

        protected ProxyBase()
        {
        }

        protected ProxyBase(Type contract, IClientPipeline pipeline)
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

        protected IClientPipeline Pipeline
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
            ClientActionContext ctxt = CreateContext(BoltFramework.SessionMetadata.Resolve(Contract).InitSession.Action, null);
            try
            {
                await Pipeline.Instance(ctxt);
                State = ProxyState.Open;
            }
            finally
            {
                ReleaseContext(ctxt);
            }
        }

        public async Task CloseAsync()
        {
            if (State == ProxyState.Open)
            {
                ClientActionContext ctxt =  CreateContext(BoltFramework.SessionMetadata.Resolve(Contract).DestroySession.Action, null);
                try
                {
                    await Pipeline.Instance(ctxt);
                    State = ProxyState.Closed;
                }
                finally
                {
                    ReleaseContext(ctxt);
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
            ClientActionContext ctxt = CreateContext(action, parameters);
            try
            {
                await Pipeline.Instance(ctxt);
                return ctxt.ActionResult;
            }
            finally
            {
                ReleaseContext(ctxt);
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

        private ClientActionContext CreateContext(MethodInfo action, params object[] parameters)
        {
            ClientActionContext context;

            if (!_contexts.TryDequeue(out context))
            {
                context = new ClientActionContext();
            }

            context.Init(this, Contract, action, parameters);
            return context;
        }

        private void ReleaseContext(ClientActionContext context)
        {
            if (_contexts.Count < _poolSize)
            {
                context.Reset();
                _contexts.Enqueue(context);
            }
        }
    }
}
