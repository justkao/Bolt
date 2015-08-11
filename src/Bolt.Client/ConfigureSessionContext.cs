using System;

using Bolt.Session;

namespace Bolt.Client
{
    public class ConfigureSessionContext
    {
        public ConfigureSessionContext(ISerializer serializer, InitSessionParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            Serializer = serializer;
            Parameters = parameters;
        }

        public ISerializer Serializer { get; }

        public InitSessionParameters Parameters { get; }

        public void Write<T>(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Parameters.Write(Serializer, key, value);
        }

        public T Read<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Parameters.Read<T>(Serializer, key);
        }
    }
}