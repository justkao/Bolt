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

        public Type Type { get; }

        public string Name { get; }
    }
}