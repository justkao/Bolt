using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        }

        public static string Generate(ContractDefinition definition)
        {
            ContractGenerator generator = new ContractGenerator();
            generator.ContractDefinition = definition;
            generator.Generate();

            return generator.Output.GetStringBuilder().ToString();
        }

        public string BaseClass { get; set; }

        public override void Generate()
        {
            ContractDescriptorGenerator contractDescriptorGenerator = new ContractDescriptorGenerator(Output, Formatter, IntendProvider);
            contractDescriptorGenerator.MetadataProvider = MetadataProvider;
            contractDescriptorGenerator.ContractDefinition = ContractDefinition;
            contractDescriptorGenerator.Generate();

            IReadOnlyCollection<Type> contracts = ContractDefinition.GetEffectiveContracts();
            ParametersGenerator generator = new ParametersGenerator(Output, Formatter, IntendProvider);
            generator.BaseClass = BaseClass;

            foreach (Type type in contracts)
            {
                if (ContractDefinition.GetEffectiveMethods(type).Any(HasParameters))
                {
                    BeginNamespace(MetadataProvider.GetParameterDescriptor(type, null).Namespace);
                    IEnumerable<MethodInfo> methods = ContractDefinition.GetEffectiveMethods(type).ToList();

                    foreach (MethodInfo method in methods)
                    {
                        if (generator.Generate(method, MetadataProvider))
                        {
                            if (!Equals(method, methods.Last()))
                            {
                                WriteLine();
                            }
                        }
                    }

                    EndNamespace();
                    WriteLine();
                }
            }
        }
    }
}