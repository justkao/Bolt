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
            generator.Contract = definition;
            generator.Generate();

            return generator.Output.GetStringBuilder().ToString();
        }

        public string BaseClass { get; set; }

        public override void Generate()
        {
            TypeDescriptorGenerator typeDescriptorGenerator = new TypeDescriptorGenerator(Output, Formatter, IntendProvider);
            typeDescriptorGenerator.MetadataProvider = MetadataProvider;
            typeDescriptorGenerator.Contract = Contract;
            typeDescriptorGenerator.Generate();

            IReadOnlyCollection<Type> contracts = Contract.GetEffectiveContracts();
            ParametersGenerator generator = new ParametersGenerator(Output, Formatter, IntendProvider);
            generator.BaseClass = BaseClass;

            foreach (Type type in contracts)
            {
                if (type.GetMethods().Any(HasParameters))
                {
                    BeginNamespace(MetadataProvider.GetParametersClass(type, null).Namespace);

                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (generator.Generate(method, MetadataProvider))
                        {
                            if (method != type.GetMethods().Last())
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