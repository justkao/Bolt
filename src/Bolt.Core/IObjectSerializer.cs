using System;
using System.IO;

namespace Bolt.Core
{
    public interface IObjectSerializer
    {
        bool HasValues();

        void AddValue(string key, Type type, object value);

        Stream Serialize();
    }
}