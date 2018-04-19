﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Metadata;

namespace Bolt.Client
{
    public class ProxyBase : IProxy, IPipelineCallback
    {
        private readonly ConcurrentQueue<ClientActionContext> _contexts = new ConcurrentQueue<ClientActionContext>();
        private readonly int _poolSize = 128;
        private ContractMetadata _contract;
        private IClientPipeline _pipeline;

        public ProxyBase()
        {
            State = ProxyState.Default;
        }

        public ProxyBase(Type contract, IClientPipeline pipeline)
        {
            State = ProxyState.Default;
            _contract = BoltFramework.GetContract(contract);
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public ProxyBase(ProxyBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            State = ProxyState.Default;
            _contract = proxy.Contract;
            _pipeline = proxy.Pipeline;
        }

        public ContractMetadata Contract
        {
            get
            {
                return _contract;
            }

            set
            {
                EnsureDefault();
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
                EnsureDefault();
                _pipeline = value;
            }
        }

        public async Task OpenAsync()
        {
            ClientActionContext ctxt = CreateContext(Contract.Session.InitSession, null);
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
                ClientActionContext ctxt = CreateContext(Contract.Session.DestroySession, null);
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
            ClientActionContext ctxt = CreateContext(Contract.GetAction(action), parameters);
            try
            {
                await Pipeline.Instance(ctxt).ConfigureAwait(false);
                if (ctxt.Action.HasResult)
                {
                    return ctxt.ActionResult;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                ReleaseContext(ctxt);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

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

        private ClientActionContext CreateContext(ActionMetadata action, object[] parameters)
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

        private void EnsureDefault()
        {
            if (State != ProxyState.Default)
            {
                throw new InvalidOperationException("Enable to update the proxy because it has already been used for communication.");
            }
        }
    }
}
