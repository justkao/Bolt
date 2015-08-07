using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public partial class ProxyGenerator
    {
        private void GenerateStaticActions(ClassGenerator generator)
        {
            foreach (MethodInfo effectiveMethod in ContractDefinition.GetEffectiveMethods())
            {
                generator.WriteLine(
                    "private static readonly {0} {1} = typeof({2}).GetMethod(\"{3}\");",
                    FormatType<MethodInfo>(),
                    GetStaticActionName(effectiveMethod),
                    ContractDefinition.Root.FullName,
                    effectiveMethod.Name);
            }
        }

        private static string GetStaticActionName(MethodInfo effectiveMethod)
        {
            return $"__{effectiveMethod.Name}Action";
        }
    }
}
