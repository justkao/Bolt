using System;
using Bolt.Server;

// ReSharper disable once CheckNamespace
namespace Bolt.Session
{
    public static class BoltServerSessionExtensions
    {
        public static T Read<T>(this SessionParametersBase parameters, ActionContextBase context, string key)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            ServerActionContext ctxt = (ServerActionContext) context;
            ISerializer serializer = ctxt.HttpContext.GetFeature<IBoltFeature>().Configuration.Serializer;

            return parameters.Read<T>(serializer, key);
        }

        public static void Write<T>(this SessionParametersBase parameters, ActionContextBase context, string key, T value)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            ServerActionContext ctxt = (ServerActionContext)context;
            ISerializer serializer = ctxt.HttpContext.GetFeature<IBoltFeature>().Configuration.Serializer;
            parameters.Write(serializer, key, value);
        }
    }
}