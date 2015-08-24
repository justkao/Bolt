using System;

namespace Bolt
{
    public interface IObjectSerializer : IDisposable
    {
        bool IsEmpty { get; }

        void Write(string key, Type type, object value);

        bool TryRead(string key, Type type, out object value);
    }
}