using System;
using Bolt.Client.Pipeline;
using Bolt.Session;

namespace Bolt.Client
{
    public class ConfigureSessionContext
    {
        public ConfigureSessionContext(SessionMiddleware middleware, InitSessionParameters parameters)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            Middleware = middleware;
            Parameters = parameters;
        }

        public SessionMiddleware Middleware { get; }

        public InitSessionParameters Parameters { get; }

        public void Write<T>(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Parameters.Write(Middleware.Serializer, key, value);
        }

        public T Read<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Parameters.Read<T>(Middleware.Serializer, key);
        }
    }
}