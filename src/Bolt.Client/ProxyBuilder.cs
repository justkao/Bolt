using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Bolt.Session;

namespace Bolt.Client
{
    public class ProxyBuilder
    {
        private readonly ClientConfiguration _configuration;
        private RetryRequestMiddleware _retryRequest;
        private SessionMiddleware _sessionMiddleware;
        private IServerProvider _serverProvider;
        private HttpMessageHandler _messageHandler;

        public ProxyBuilder(ClientConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration;
        }

        public virtual ProxyBuilder Recoverable(int retries, TimeSpan retryDelay)
        {
            _retryRequest = new RetryRequestMiddleware(_configuration.ErrorRecovery)
            {
                Retries = retries,
                RetryDelay = retryDelay
            };

            return this;
        }

        public virtual ProxyBuilder UseSession(Action<ConfigureSessionContext> configureSession = null, bool distributed = false)
        {
            _sessionMiddleware = new SessionMiddleware(_configuration.Serializer, _configuration.SessionHandler)
            {
                UseDistributedSession = distributed,
                InitSessionParameters = new InitSessionParameters()
            };
            configureSession?.Invoke(new ConfigureSessionContext(_sessionMiddleware, _sessionMiddleware.InitSessionParameters));

            return this;
        }

        public virtual ProxyBuilder Url(IServerProvider serverProvider)
        {
            if (serverProvider == null)
            {
                throw new ArgumentNullException(nameof(serverProvider));
            }

            _serverProvider = serverProvider;
            return this;
        }

        public virtual ProxyBuilder Url(params string[] servers)
        {
            if (servers == null)
            {
                throw new ArgumentNullException(nameof(servers));
            }

            return Url(servers.Select(s => new Uri(s)).ToArray());
        }

        public virtual ProxyBuilder Url(params Uri[] servers)
        {
            if (servers == null || !servers.Any())
            {
                throw new ArgumentNullException(nameof(servers));
            }

            if (servers.Length == 1)
            {
                _serverProvider = new SingleServerProvider(servers[0]);
            }
            else
            {
                _serverProvider = new MultipleServersProvider(servers);
            }

            return this;
        }

        public virtual ProxyBuilder UseHttpMessageHandler(HttpMessageHandler messageHandler)
        {
            if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler = messageHandler;
            return this;
        }

        public virtual TContract Build<TContract>() where TContract : class
        {
            if (_serverProvider == null)
            {
                throw new InvalidOperationException("Server provider or target url was not configured.");
            }

            PipelineBuilder<ClientActionContext> context = new PipelineBuilder<ClientActionContext>();

            context.Use(new ValidateProxyMiddleware());
            if (_retryRequest != null)
            {
                context.Use(_retryRequest);
            }

            if (_sessionMiddleware != null)
            {
                context.Use(_sessionMiddleware);
            }

            context.Use(new SerializationMiddleware(_configuration.Serializer, _configuration.ExceptionWrapper, _configuration.ErrorProvider));
            context.Use(new CommunicationMiddleware(_messageHandler ?? new HttpClientHandler()));
            PipelineResult<ClientActionContext> result = context.Build();

            return _configuration.ProxyFactory.CreateProxy<TContract>(result);
        }
    }
}
