using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Bolt.Client.Filters;
using Bolt.Client.Helpers;
using Bolt.Core;
using Bolt.Session;

namespace Bolt.Client.Pipeline
{
    public class SessionHandler : IClientContextHandler
    {
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();

        private string _sessionId;

        public SessionHandler(ISerializer serializer, IClientSessionHandler sessionHandler)
        {
            Serializer = serializer;
            ClientSessionHandler = sessionHandler;
        }

        public ISerializer Serializer { get;  }

        public IClientSessionHandler ClientSessionHandler { get;  }

        public HandleContextStage Stage => HandleContextStage.Before;

        public InitSessionParameters InitSessionParameters { get; set; }

        public DestroySessionParameters DestroySessionParameters { get; set; }

        public InitSessionResult InitSessionResult { get; set; }

        public DestroySessionResult DestroySessionResult { get; set; }

        public bool UseDistributedSession { get; set; }

        public ConnectionDescriptor Connection { get; set; }

        public bool IsOpened { get; set; }

        public async Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            EnsureNotClosed();

            if (!IsOpened)
            {
                Connection = await EnsureConnectionAsync(context, next);
            }

            if (IsOpened && (!Equals(context.Action, BoltFramework.DestroySessionAction)))
            {
                ClientSessionHandler.EnsureSession(context.Request, null);

                if (!UseDistributedSession)
                {
                    // we stick to active connection otherwise new connection will be picked using ServerConnectionHandler
                    context.Connection = Connection;
                }

                try
                {
                    await next(context);
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
                byte[] raw = Serializer.Serialize(DestroySessionParameters ?? new DestroySessionParameters()).ToArray();
                context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Serializer.ContentType));
                context.Request.Content = new ByteArrayContent(raw);
                context.Request.Content.Headers.ContentLength = raw.Length;

                await next(context);
                DestroySessionResult = (DestroySessionResult) context.ActionResult;
                IsOpened = false;
                return;
            }


            await next(context);
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
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
                        Action = BoltFramework.InitSessionAction
                    };

                    using (initSessionContext)
                    {
                        ClientSessionHandler.EnsureSession(initSessionContext.Request, _sessionId);

                        byte[] raw = Serializer.Serialize(initSessionParameters).ToArray();
                        context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Serializer.ContentType));
                        context.Request.Content = new ByteArrayContent(raw);
                        context.Request.Content.Headers.ContentLength = raw.Length;

                        await next(initSessionContext);

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
