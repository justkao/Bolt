using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Client
{
    public abstract class ProxyBase : IChannel
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

        public Type Contract { get; protected set; }

        public ChannelState State { get; private set; }

        protected IPipeline<ClientActionContext> Pipeline { get; set; }

        public async Task OpenAsync()
        {
            using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.InitSessionAction, new object[] { null }))
            {
                await Pipeline.Instance(ctxt);
                State = ChannelState.Open;
            }
        }

        public async Task CloseAsync()
        {
            if (State == ChannelState.Open)
            {
                using (ClientActionContext ctxt = new ClientActionContext(this, Contract, BoltFramework.DestroySessionAction, new object[] { null }))
                {
                    await Pipeline.Instance(ctxt);
                    State = ChannelState.Closed;
                }
            }

            Dispose();
        }

        public async Task<object> SendAsync(MethodInfo action, object[] parameters)
        {
            using (ClientActionContext ctxt = new ClientActionContext(this, Contract, action, parameters))
            {
                await Pipeline.Instance(ctxt);
                return ctxt.ActionResult;
            }
        }

        public void Dispose()
        {
            if (State == ChannelState.Open)
            {
                this.Close();
            }

            Pipeline?.Dispose();
            Pipeline = null;
        }
    }
}
