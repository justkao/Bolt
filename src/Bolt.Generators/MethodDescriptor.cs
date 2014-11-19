using System.Reflection;

namespace Bolt.Generators
{
    public class MethodDescriptor
    {
        public ContractDefinition Contract { get; set; }

        public MethodInfo Method { get; set; }

        public string Name { get; set; }
    }
}