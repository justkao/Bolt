using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using System.Collections.Concurrent;

namespace Bolt.Client
{
    public class ProxyBase : IProxy, IPipelineCallback
    {
        private IClientPipeline _pipeline;
        private readonly ConcurrentQueue<ClientActionContext> _contexts = new ConcurrentQueue<ClientActionContext>();
        private readonly int _poolSize = Environment.ProcessorCount * 5;
        private Type _contract;

        public ProxyBase()
        {
            State = ProxyState.Ready;
        }

        public ProxyBase(Type contract, IClientPipeline pipeline)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            State = ProxyState.Ready;
            _contract = contract;
            _pipeline = pipeline;
        }

        public ProxyBase(ProxyBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            State = ProxyState.Ready;
            _contract = proxy.Contract;
            _pipeline = proxy.Pipeline;
        }

        public Type Contract
        {
            get { return _contract; }
            set
            {
                EnsureReady();
                _contract = value;
            }
        }

        public ProxyState State { get; private set; }

        public IClientPipeline Pipeline
        {
            get
            {
                if (_pipeline == null)
                {
                    throw new ProxyClosedException();
                }

                return _pipeline;
            }

            set
            {
                EnsureReady();
                _pipeline = value;
            }
        }

        public async Task OpenAsync()
        {
            ClientActionContext ctxt = CreateContext(BoltFramework.SessionMetadata.Resolve(Contract).InitSession.Action, null);
            try
            {
                await Pipeline.Instance(ctxt).ConfigureAwait(false);
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
                    await Pipeline.Instance(ctxt).ConfigureAwait(false);
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
                await Pipeline.Instance(ctxt).ConfigureAwait(false);
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

        private void EnsureReady()
        {
            if (State != ProxyState.Ready)
            {
                throw new InvalidOperationException("Enable to update the proxy because it has already been used for communication.");
            }
        }
    }
}
