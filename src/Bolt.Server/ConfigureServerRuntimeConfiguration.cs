using System;

using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class ConfigureServerRuntimeConfiguration : ConfigureOptions<ServerRuntimeConfiguration>
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public ConfigureServerRuntimeConfiguration(IServiceProvider provider, ILoggerFactory loggerFactory) : base(null)
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

        public override void Configure(ServerRuntimeConfiguration options, string name = "")
        {
            _logger.LogInformation(BoltLogId.ConfigureDefaultServerRuntimeConfiguration, "Configuring default server runtime configuration.");

            options.Options = _provider.GetRequiredService<IOptions<BoltServerOptions>>().Options;
            options.Serializer = _provider.GetRequiredService<ISerializer>();
            options.ExceptionWrapper = _provider.GetRequiredService<IExceptionWrapper>();
        }
    }
}