using System;

namespace Bolt
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; private set; }

        public string Name { get; private set; }
    }
}