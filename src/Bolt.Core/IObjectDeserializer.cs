using System;

namespace Bolt.Core
{
    public interface IObjectDeserializer
    {
        object GetValue(string key, Type type);
    }
}