using System;
using System.Reflection;

namespace Bolt
{
    public interface IMetadataProvider
    {
        ClassDescriptor GetParametersClass(Type owner, MethodInfo method);

        MethodDescriptor GetRemoteUrl(ContractDefinition definition, MethodInfo method);

        string GetRemoteUrl(ContractDefinition definition);
    }

    public class MetadataProvider : IMetadataProvider
    {
        public ClassDescriptor GetParametersClass(Type owner, MethodInfo method)
        {
            string ns = owner.Namespace + ".Parameters";
            string name = method != null ? method.Name + "Parameters" : null;

            return new ClassDescriptor()
            {
                Namespace = ns,
                Name = name,
                FullName = ns + "." + name
            };
        }

        public MethodDescriptor GetRemoteUrl(ContractDefinition definition, MethodInfo method)
        {
            return new MethodDescriptor(
                definition.RootContract.StripInterfaceName(),
                method.Name,
                definition.RootContract.StripInterfaceName() + "/" + method.Name);
        }

        public string GetRemoteUrl(ContractDefinition definition)
        {
            return definition.RootContract.StripInterfaceName();
        }
    }
}
