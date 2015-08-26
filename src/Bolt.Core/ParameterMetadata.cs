using System;

namespace Bolt
{
    public class ParameterMetadata
    {
        public ParameterMetadata(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; private set; }

        public string Name { get; private set; }
    }
}