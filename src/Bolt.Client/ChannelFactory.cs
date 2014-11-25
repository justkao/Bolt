using System;

namespace Bolt.Client
{
    public class ChannelFactory<TChannel>
        where TChannel : Channel, new()
    {
        public ClientConfiguration ClientConfiguration { get; set; }

        public string Prefix { get; set; }

        public virtual TChannel Create(Uri server)
        {
            return Create(new ConnectionProvider(server));
        }

        public virtual TChannel CreateStateFull(Uri server)
        {
            return Create(new StateFullConnectionProvider(server));
        }

        public virtual TChannel Create(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null)
            {
                throw new ArgumentNullException("connectionProvider");
            }

            if (ClientConfiguration == null)
            {
                throw new InvalidOperationException("ClientConfiguration not initialized.");
            }


            TChannel channel = new TChannel();
            ClientConfiguration.Update(channel);
            channel.Prefix = Prefix;
            channel.ConnectionProvider = connectionProvider;

            return channel;
        }
    }
}