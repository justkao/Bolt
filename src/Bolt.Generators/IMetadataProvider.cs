using System.Reflection;

namespace Bolt.Generators
{
    public interface IMetadataProvider
    {
        MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method);

        ClassDescriptor GetContractDescriptor(ContractDefinition contract);
    }

    public class MetadataProvider : IMetadataProvider
    {
        public MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method)
        {
            return new MethodDescriptor(contract, method, method.Name, GetParameterDescriptor(contract, method));
        }

        public ClassDescriptor GetContractDescriptor(ContractDefinition contract)
        {
            return new ClassDescriptor(contract.Name + "Descriptor", contract.Namespace);
        }

        protected virtual ClassDescriptor GetParameterDescriptor(ContractDefinition contract, MethodInfo method)
        {
            if (method.GetParameters().Length == 0)
            {
                return null;
            }

            string ns = method.DeclaringType.Namespace + ".Parameters";
            string name = method.Name + "Parameters";

            return new ClassDescriptor(name, ns);
        }
    }
}
