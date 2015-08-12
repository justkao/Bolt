using System;
using System.IO;

namespace Bolt
{
    public interface IObjectSerializer
    {
        bool IsEmpty { get; }

        void Write(string key, Type type, object value);

        bool TryRead(string key, Type type, out object value);

        Stream GetOutputStream();
    }
}