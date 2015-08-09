using System.Reflection;

namespace Bolt.Generators
{
    public interface IMetadataProvider
    {
        MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method);
    }

    public class MetadataProvider : IMetadataProvider
    {
        public MethodDescriptor GetMethodDescriptor(ContractDefinition contract, MethodInfo method)
        {
            return new MethodDescriptor(contract, method, method.Name);
        }
    }
}
