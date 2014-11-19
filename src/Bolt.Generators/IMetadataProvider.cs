using System;
using System.Reflection;

namespace Bolt.Generators
{
    public interface IMetadataProvider
    {
        ParametersDescriptor GetParametersClass(Type owner, MethodInfo method);

        MethodDescriptor GetMethodDescriptor(ContractDefinition definition, MethodInfo method);

        string GetRemoteUrl(ContractDefinition definition);

        TypeDescriptor GetTypeDescriptor(Type type);
    }

    public class MetadataProvider : IMetadataProvider
    {
        public ParametersDescriptor GetParametersClass(Type owner, MethodInfo method)
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

        public MethodDescriptor GetMethodDescriptor(ContractDefinition definition, MethodInfo method)
        {
            return new MethodDescriptor(
                definition.RootContract.StripInterfaceName(),
                method.Name,
                definition.RootContract.StripInterfaceName() + "/" + method.Name, null);
        }

        public string GetRemoteUrl(ContractDefinition definition)
        {
            return definition.RootContract.StripInterfaceName();
        }

        public TypeDescriptor GetTypeDescriptor(Type type)
        {
            return new TypeDescriptor()
            {
                Name = type.StripInterfaceName() + "Descriptor",
                Namespace = type.Namespace,
                FullName = type.Namespace + "." + type.StripInterfaceName() + "Descriptor"
            };
        }
    }
}
