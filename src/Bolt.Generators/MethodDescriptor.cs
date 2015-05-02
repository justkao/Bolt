using System.Collections.Generic;
using System.Linq;
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

        public MethodInfo Method { get; }

        public string Name { get; private set; }

        public ClassDescriptor Parameters { get; }

        public bool HasParameterClass()
        {
            return Parameters != null && Parameters.FullName != BoltConstants.Core.Empty.FullName;
        }

        public IEnumerable<ParameterInfo> GetAllParameters()
        {
            return Method.GetParameters();
        }

        public IEnumerable<ParameterInfo> GetParameters()
        {
            return Method.GetParameters().Where(p => !typeof(CancellationToken).GetTypeInfo().IsAssignableFrom(p.ParameterType.GetTypeInfo()));
        }

        public ParameterInfo GetCancellationTokenParameter()
        {
            return
                Method.GetParameters().FirstOrDefault(p => typeof(CancellationToken).GetTypeInfo().IsAssignableFrom(p.ParameterType.GetTypeInfo()));
        }
    }
}