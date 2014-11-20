using System.Reflection;

namespace Bolt.Generators
{
    public class MethodDescriptor
    {
        public MethodDescriptor(ContractDefinition contract, MethodInfo method, string name, ClassDescriptor parameters)
        {
            Contract = contract;
            Method = method;
            Name = name;
            Parameters = parameters;
        }

        public ContractDefinition Contract { get; private set; }

        public MethodInfo Method { get; private set; }

        public string Name { get; private set; }

        public ClassDescriptor Parameters { get; private set; }

        public bool HasParameters()
        {
            return Parameters != null && Parameters.FullName != typeof(Empty).FullName;
        }
    }
}