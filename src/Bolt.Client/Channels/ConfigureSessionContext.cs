using System;
using System.IO;
using System.Text;
using Bolt.Session;

namespace Bolt.Client.Channels
{
    public class ConfigureSessionContext
    {
        public ConfigureSessionContext(SessionChannel channel, InitSessionParameters parameters)
        {
            Channel = channel;
            Parameters = parameters;
        }

        public SessionChannel Channel { get; }

        public InitSessionParameters Parameters { get; }

        public void Write<T>(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Parameters.Write(Channel.Serializer, key, value);
        }

        public T Read<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Parameters.Read<T>(Channel.Serializer, key);
        }
    }
}