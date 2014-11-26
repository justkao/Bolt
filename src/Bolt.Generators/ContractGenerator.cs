using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolt.Generators
{
    public class ContractGenerator : ContractGeneratorBase
    {
        public ContractGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ContractGenerator(StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(output, formatter, provider)
        {
            Modifier = "public";
        }

        public string Modifier { get; set; }

        public override void Generate()
        {
            List<MethodDescriptor> methods =
                ContractDefinition.GetEffectiveMethods()
                    .Select(m => MetadataProvider.GetMethodDescriptor(ContractDefinition, m))
                    .Where(m => m.HasParameterClass())
                    .ToList();

            foreach (IGrouping<string, MethodDescriptor> grouping in methods.GroupBy(m => m.Parameters.Namespace))
            {
                using (WithNamespace(grouping.Key))
                {
                    foreach (MethodDescriptor method in methods)
                    {
                        ParametersGenerator parametersGenerator = new ParametersGenerator(method, Output, Formatter, IntendProvider)
                        {
                            IncludeNamespace = false,
                        };

                        parametersGenerator.Modifier = Modifier;
                        if (!method.HasParameterClass())
                        {
                            continue;
                        }

                        parametersGenerator.Generate();
                    }
                }

                WriteLine();
            }
        }
    }
}