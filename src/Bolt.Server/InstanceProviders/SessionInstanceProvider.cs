using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bolt.Server.Session;

namespace Bolt.Server.InstanceProviders
{
    public class SessionInstanceProvider : InstanceProvider
    {
        private readonly ISessionFactory _sessionFactory;

        public SessionInstanceProvider(ISessionFactory factory)
        {
            _sessionFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public sealed override async Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            IContractSession contractSession;
            
            if (context.Action == BoltFramework.SessionMetadata.Resolve(context.Contract).InitSession.Action)
            {
                contractSession = await _sessionFactory.CreateAsync(context.HttpContext, await base.GetInstanceAsync(context, type));
                context.ContractInstance = contractSession.Instance;
                context.HttpContext.Features.Set<IContractSession>(contractSession);

                await OnInstanceCreatedAsync(context, contractSession.SessionId);
                return contractSession.Instance;
            }

            contractSession = await _sessionFactory.GetExistingAsync(context.HttpContext, () => base.GetInstanceAsync(context, type));
            context.HttpContext.Features.Set<IContractSession>(contractSession);
            return contractSession.Instance;
        }

        public sealed override async Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            var session = context.HttpContext.Features.Get<IContractSession>();
            if (session == null)
            {
                return;
            }

            if (context.Action == BoltFramework.SessionMetadata.Resolve(context.Contract).InitSession.Action)
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
            else if (context.Action == BoltFramework.SessionMetadata.Resolve(context.Contract).DestroySession.Action)
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