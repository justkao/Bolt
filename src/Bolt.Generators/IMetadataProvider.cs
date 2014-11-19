using System;
using System.Reflection;

namespace Bolt.Generators
{
    public interface IMetadataProvider
    {
        ParametersDescriptor GetParameterDescriptor(Type owner, MethodInfo method);

        MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method);

        TypeDescriptor GetContractDescriptor(ContractDefinition contract);
    }

    public class MetadataProvider : IMetadataProvider
    {
        public ParametersDescriptor GetParameterDescriptor(Type owner, MethodInfo method)
        {
            string ns = owner.Namespace + ".Parameters";
            string name = method != null ? method.Name + "Parameters" : null;

            return new ParametersDescriptor()
            {
                Namespace = ns,
                Name = name,
                FullName = ns + "." + name
            };
        }

        public MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method)
        {
            return new MethodDescriptor() { Contract = contract, Name = method.Name, Method = method };
        }

        public TypeDescriptor GetContractDescriptor(ContractDefinition contract)
        {
            return new TypeDescriptor()
            {
                Name = contract.Name + "Descriptor",
                Namespace = contract.Namespace,
                FullName = contract.Namespace + "." + contract.Name + "Descriptor"
            };
        }
    }
}
