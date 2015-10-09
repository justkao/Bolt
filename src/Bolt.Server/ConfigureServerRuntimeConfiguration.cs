using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System.Collections.Generic;

namespace Bolt.Server
{
    public class ConfigureServerRuntimeConfiguration : ConfigureOptions<ServerRuntimeConfiguration>
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public ConfigureServerRuntimeConfiguration(IServiceProvider provider, ILoggerFactory loggerFactory)
            : base((o) => { })
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _provider = provider;
            _logger = loggerFactory.CreateLogger<ConfigureServerRuntimeConfiguration>();
        }

        public override void Configure(ServerRuntimeConfiguration options)
        {
            _logger.LogInformation(BoltLogId.ConfigureDefaultServerRuntimeConfiguration,
                "Configuring default server runtime configuration.");

            options.Options = _provider.GetRequiredService<IOptions<BoltServerOptions>>().Value;
            options.AvailableSerializers = _provider.GetRequiredService<IEnumerable<ISerializer>>().ToList();
            options.ExceptionWrapper = _provider.GetRequiredService<IExceptionWrapper>();
            options.ErrorHandler = _provider.GetRequiredService<IServerErrorHandler>();
        }
    }
}