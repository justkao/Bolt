using System;

namespace Bolt.Session
{
    public static class SessionExtensions
    {
        public static T Read<T>(this SessionParametersBase parameters, ISerializer serializer, string key)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string val;
            if (parameters.UserData.TryGetValue(key, out val))
            {
                return serializer.Read<T>(val);
            }

            return default(T);
        }

        public static void Write<T>(this SessionParametersBase parameters, ISerializer serializer, string key, T value)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            parameters.UserData[key] = serializer.Write(value);
        }
    }
}