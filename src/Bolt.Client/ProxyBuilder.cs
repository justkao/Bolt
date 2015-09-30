using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;

namespace Bolt.Client
{
    public class ProxyBuilder
    {
        private readonly ClientConfiguration _configuration;
        private RetryRequestMiddleware _retryRequest;
        private SessionMiddleware _sessionMiddleware;
        private IServerProvider _serverProvider;
        private HttpMessageHandler _messageHandler;
        private AcceptLanguageMiddleware _acceptLanguageMiddleware;
        private StreamingMiddleware _streamingMiddleware;
        private TimeSpan? _timeout;
        private IRequestTimeoutProvider _timeoutProvider;

        private readonly List<IMiddleware<ClientActionContext>> _beforeSend = new List<IMiddleware<ClientActionContext>>();

        public ProxyBuilder(ClientConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration;
        }

        public virtual ProxyBuilder Recoverable(int retries, TimeSpan retryDelay, IErrorHandling errorHandling = null)
        {
            _retryRequest = new RetryRequestMiddleware(errorHandling ?? _configuration.ErrorHandling)
            {
                Retries = retries,
                RetryDelay = retryDelay
            };

            return this;
        }

        public virtual ProxyBuilder UseSession(bool distributed = false, IErrorHandling errorHandling = null)
        {
            _sessionMiddleware = new SessionMiddleware(_configuration.SessionHandler, errorHandling ?? _configuration.ErrorHandling)
                                     {
                                         UseDistributedSession = distributed
                                     };
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

        public virtual ProxyBuilder Timeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public virtual ProxyBuilder Timeout(IRequestTimeoutProvider timeoutProvider)
        {
            _timeoutProvider = timeoutProvider;
            return this;
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

        public virtual ProxyBuilder PreserveCultureInfo()
        {
            _acceptLanguageMiddleware = new AcceptLanguageMiddleware();

            return this;
        }

        public virtual ProxyBuilder OnSending(Func<ActionDelegate<ClientActionContext>, ClientActionContext, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _beforeSend.Add(new DelegatedMiddleware<ClientActionContext>(handler));
            return this;
        }

        public virtual ProxyBuilder OnSending(IMiddleware<ClientActionContext> middleware)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));

            _beforeSend.Add(middleware);
            return this;
        }

        public virtual ProxyBuilder UseStreaming()
        {
            _streamingMiddleware = new StreamingMiddleware();
            return this;
        }

        public virtual IClientPipeline BuildPipeline()
        {
            if (_serverProvider == null)
            {
                throw new InvalidOperationException("Server provider or target url was not configured.");
            }

            ClientPipelineBuilder context = new ClientPipelineBuilder();

            context.Use(new ValidateProxyMiddleware());
            if (_retryRequest != null)
            {
                context.Use(_retryRequest);
            }

            if (_sessionMiddleware != null)
            {
                context.Use(_sessionMiddleware);
            }

            if (_streamingMiddleware != null)
            {
                context.Use(_streamingMiddleware);
            }

            context.Use(new SerializationMiddleware(_configuration.Serializer, _configuration.ExceptionWrapper, _configuration.ErrorProvider));
            if (_acceptLanguageMiddleware != null)
            {
                context.Use(_acceptLanguageMiddleware);
            }

            context.Use(new PickConnectionMiddleware(_serverProvider, _configuration.EndpointProvider));
            foreach (IMiddleware<ClientActionContext> middleware in _beforeSend)
            {
                context.Use(middleware);
            }

            context.Use(
                new CommunicationMiddleware(_messageHandler ??
                                            _configuration.HttpMessageHandler ?? new HttpClientHandler())
                {
                    ResponseTimeout = _timeout ?? _configuration.DefaultResponseTimeout,
                    TimeoutProvider = _timeoutProvider ?? _configuration.TimeoutProvider
                });

            return (IClientPipeline)context.Build();
        }

        public virtual TContract Build<TContract>() where TContract : class
        {
            IClientPipeline pipeline = BuildPipeline();
            TContract proxy = _configuration.ProxyFactory.CreateProxy<TContract>(pipeline);
            if (proxy is IContractProvider)
            {
                BoltFramework.ValidateContract((proxy as IContractProvider).Contract);
                pipeline.Validate((proxy as IContractProvider).Contract);
            }
            else
            {
                BoltFramework.ValidateContract(typeof(TContract));
                pipeline.Validate(typeof(TContract));
            }


            return proxy;
        }
    }
}
