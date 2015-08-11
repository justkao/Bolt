using System;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Bolt.Client.Helpers;
using Bolt.Session;

namespace Bolt.Client.Pipeline
{
    public class SessionMiddleware : ClientMiddlewareBase
    {
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();

        private string _sessionId;

        public SessionMiddleware(ISerializer serializer, IClientSessionHandler sessionHandler)
        {
            Serializer = serializer;
            ClientSessionHandler = sessionHandler;
        }

        public ISerializer Serializer { get;  }

        public IClientSessionHandler ClientSessionHandler { get;  }

        public InitSessionParameters InitSessionParameters { get; set; }

        public DestroySessionParameters DestroySessionParameters { get; set; }

        public InitSessionResult InitSessionResult { get; set; }

        public DestroySessionResult DestroySessionResult { get; set; }

        public bool UseDistributedSession { get; set; }

        public ConnectionDescriptor Connection { get; set; }

        public bool IsOpened { get; set; }

        public override async Task Invoke(ClientActionContext context)
        {
            EnsureNotClosed();

            if (!IsOpened)
            {
                Connection = await EnsureConnectionAsync(context);
            }

            if (IsOpened && (!Equals(context.Action, BoltFramework.DestroySessionAction)))
            {
                ClientSessionHandler.EnsureSession(context.Request, null);

                if (!UseDistributedSession)
                {
                    // we stick to active connection otherwise new connection will be picked using PickConnectionMiddleware
                    context.Connection = Connection;
                }

                try
                {
                    await Next(context);
                }
                catch (Exception e)
                {
                    if (ShouldCloseConnection(e))
                    {
                        Connection = null;
                    }

                    throw;
                }
                return;
            }

            if (Equals(context.Action, BoltFramework.DestroySessionAction))
            {
                context.Parameters = new object[] {DestroySessionParameters ?? new DestroySessionParameters()};
                await Next(context);
                DestroySessionResult = (DestroySessionResult) context.ActionResult;
                IsOpened = false;
                return;
            }

            await Next(context);
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context)
        {
            EnsureNotClosed();

            if (IsOpened)
            {
                return Connection;
            }

            using (await _syncRoot.EnterAsync())
            {
                if (Connection != null)
                {
                    return Connection;
                }

                try
                {
                    InitSessionParameters initSessionParameters = InitSessionParameters ?? new InitSessionParameters();
                    ClientActionContext initSessionContext = new ClientActionContext(context)
                    {
                        Connection = null,
                        Action = BoltFramework.InitSessionAction,
                        Parameters = new[] {(object) initSessionParameters}
                    };

                    using (initSessionContext)
                    {
                        ClientSessionHandler.EnsureSession(initSessionContext.Request, _sessionId);

                        await Next(initSessionContext);

                        string sessionId = ClientSessionHandler.GetSessionIdentifier(initSessionContext.Response);
                        if (sessionId == null)
                        {
                            throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, BoltFramework.InitSessionAction, initSessionContext.Request.RequestUri.ToString());
                        }

                        InitSessionResult = (InitSessionResult) initSessionContext.ActionResult;
                        Connection = context.Connection;
                        _sessionId = sessionId;
                    }

                    return Connection;
                }
                catch (Exception)
                {
                    Connection = null;
                    _sessionId = null;
                    throw;
                }
            }
        }

        private void EnsureNotClosed()
        {
            throw new NotImplementedException();
        }

        private bool ShouldCloseConnection(Exception exception)
        {
            throw new NotImplementedException();
        }

    }
}
