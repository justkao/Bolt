using Bolt.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider : InstanceProvider
    {
        private readonly ISessionFactory _sessionFactory;

        public StateFullInstanceProvider(ActionDescriptor initSession, ActionDescriptor closeSession, ISessionFactory factory)
        {
            if (initSession == null)
            {
                throw new ArgumentNullException(nameof(initSession));
            }

            if (closeSession == null)
            {
                throw new ArgumentNullException(nameof(closeSession));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            InitSession = initSession;
            CloseSession = closeSession;
            _sessionFactory = factory;
        }

        public ActionDescriptor InitSession { get; }

        public ActionDescriptor CloseSession { get; }

        public sealed override async Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            IContractSession contractSession;

            if (context.Action == InitSession)
            {
                contractSession = await _sessionFactory.CreateAsync(context.HttpContext, await base.GetInstanceAsync(context, type));
                context.ContractInstance = contractSession.Instance;
                context.HttpContext.SetFeature(contractSession);

                await OnInstanceCreatedAsync(context, contractSession.SessionId);
                return contractSession.Instance;
            }

            contractSession = await _sessionFactory.GetExistingAsync(context.HttpContext);
            context.HttpContext.SetFeature(contractSession);
            return context.ContractInstance;
        }

        public sealed override async Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            var session = context.HttpContext.GetFeature<IContractSession>();
            if (session == null)
            {
                return;
            }

            if (context.Action == InitSession)
            {
                if (error != null)
                {
                    try
                    {
                        await session.DestroyAsync();
                        await OnInstanceReleasedAsync(context, session.SessionId);
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(
                            false,
                            "Instance release failed after the session initialization error. This exception will be supressed and the session initialization error will be propagated to client.",
                            e.ToString());
                    }
                }
            }
            else if (context.Action == CloseSession)
            {
                await session.DestroyAsync();
                await OnInstanceReleasedAsync(context, session.SessionId);
            }
            else
            {
                await session.CommitAsync();
            } 
        }

        protected virtual Task OnInstanceCreatedAsync(ServerActionContext context, string sessionId)
        {
            return CompletedTask.Done;
        }

        protected virtual Task OnInstanceReleasedAsync(ServerActionContext context, string sessionId)
        {
            return CompletedTask.Done;
        }
    }
}