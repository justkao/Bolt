using System;
using System.Collections.Generic;
using System.Linq;

using Bolt.Client.Channels;
using Bolt.Client.Filters;

namespace Bolt.Client
{
    public class ProxyBuilder
    {
        private readonly ClientConfiguration _configuration;
        private readonly List<IClientExecutionFilter> _filters = new List<IClientExecutionFilter>();
        private IServerProvider _serverProvider;
        private Action<ConfigureSessionContext> _configureSession;
        private int _retries;
        private TimeSpan _retryDelay;
        private bool _useSession;
        private bool _distributedSession;

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
            _retries = retries;
            _retryDelay = retryDelay;

            return this;
        }

        public virtual ProxyBuilder UseSession(bool distributed = false)
        {
            _useSession = true;
            _distributedSession = distributed;
            return this;
        }

        public virtual ProxyBuilder ConfigureSession(Action<ConfigureSessionContext> configureSession)
        {
            if (configureSession == null)
            {
                throw new ArgumentNullException(nameof(configureSession));
            }

            _useSession = true;
            _configureSession = configureSession;
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

        public virtual ProxyBuilder Url(params Uri[] servers)
        {
            if (servers == null || servers.Any())
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

        public virtual ProxyBuilder Filter<T>() where T:IClientExecutionFilter, new()
        {
            _filters.Add(Activator.CreateInstance<T>());
            return this;
        }

        public virtual ProxyBuilder Filter(params IClientExecutionFilter[] filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            _filters.AddRange(filters);
            return this;
        }

        public virtual TContract Build<TContract>() where TContract : class
        {
            BoltFramework.ValidateContract(typeof(TContract));

            if (_serverProvider == null)
            {
                throw new InvalidOperationException("Server provider or target url was not configured.");
            }

            RecoverableChannel channel;

            List<IClientExecutionFilter> filters = _configuration.Filters.ToList();
            filters.AddRange(_filters);

            if (!_useSession)
            {
                channel = new RecoverableChannel(
                    _configuration.Serializer,
                    _serverProvider,
                    _configuration.RequestHandler,
                    _configuration.EndpointProvider,
                    filters)
                              {
                                  Retries = _retries, RetryDelay = _retryDelay
                              };
            }
            else
            {
                channel = new SessionChannel(
                    typeof(TContract),
                    _configuration.Serializer,
                    _serverProvider,
                    _configuration.RequestHandler,
                    _configuration.EndpointProvider,
                    _configuration.SessionHandler,
                    filters)
                              {
                                  Retries = _retries, RetryDelay = _retryDelay, UseDistributedSession = _distributedSession
                              };

                if (_configureSession != null)
                {
                    channel.ConfigureSession(_configureSession);
                }
            }

            return _configuration.ProxyFactory.CreateProxy<TContract>(channel);
        }
    }
}
