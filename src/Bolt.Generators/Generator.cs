using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bolt.Generators
{
    public class Generator : ContractGeneratorBase
    {
        private readonly List<ContractGeneratorBase> _contractGenerator = new List<ContractGeneratorBase>();

        public Generator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public Generator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public static Generator Create(ContractDefinition contract = null)
        {
            return new Generator()
            {
                ContractDefinition = contract
            };
        }

        public string GetResult()
        {
            Generate();
            return Output.GetStringBuilder().ToString();
        }

        public override void Generate()
        {
            foreach (ContractGeneratorBase generatorBase in _contractGenerator)
            {
                try
                {
                    generatorBase.Generate();
                }
                catch (Exception e)
                {
                    WriteLine("/*");
                    WriteLine("Execution of '{0}' generator failed with error '{1}'", generatorBase.GetType().Name, e.ToString());
                    WriteLine("*/");
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (string ns in Formatter.GetNamespaces())
            {
                sb.AppendFormat("using {0};\n", ns);
            }
            sb.AppendLine();

            Output.GetStringBuilder().Insert(0, sb.ToString());
        }

        public Generator Async(ContractDefinition definition = null, ClassDescriptor contractDescriptor = null, bool force = false)
        {
            return Add(new InterfaceGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition ?? ContractDefinition,
                ContractDescriptor = contractDescriptor,
                ForceAsync = force
            });
        }

        public Generator Descriptor(ContractDefinition definition = null, ClassDescriptor descriptor = null)
        {
            return
                Add(
                    new ContractDescriptorGenerator(Output, Formatter, IntendProvider)
                        {
                            ContractDefinition = definition ?? ContractDefinition,
                            ContractDescriptor = descriptor
                        });
        }

        public Generator Contract(ContractDefinition definition = null, ClassDescriptor descriptor = null)
        {
            return
                Add(
                    new ContractGenerator(Output, Formatter, IntendProvider)
                        {
                            ContractDefinition = definition ?? ContractDefinition,
                            ContractDescriptor = descriptor
                        });
        }

        public Generator Server(ContractDefinition definition = null, ClassDescriptor descriptor = null)
        {
            return
                Add(
                    new ServerGenerator(Output, Formatter, IntendProvider)
                        {
                            ContractDefinition = definition ?? ContractDefinition,
                            ContractDescriptor = descriptor
                        });
        }

        public Generator StateFullClient(ContractDefinition definition = null, ClassDescriptor descriptor = null, bool forceAsync = false)
        {
            return Add(new ClientGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition ?? ContractDefinition,
                ContractDescriptor = descriptor,
                ForceAsync = forceAsync,
                StateFull = true
            });
        }

        public Generator Client(ContractDefinition definition = null, ClassDescriptor descriptor = null, bool forceAsync = false)
        {
            return Add(new ClientGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition ?? ContractDefinition,
                ContractDescriptor = descriptor,
                ForceAsync = forceAsync
            });
        }

        private Generator Add(ContractGeneratorBase generator)
        {
            generator.MetadataProvider = MetadataProvider;
            Formatter.AddNamespace(generator.ContractDefinition.Namespace);
            _contractGenerator.Add(generator);
            return this;
        }
    }
}