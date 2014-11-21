using System.IO;

namespace Bolt.Generators
{
    public class ClientFactoryGenerator : ContractGenerator
    {
        public ClientFactoryGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public override void Generate()
        {
            CreateClassGenerator(ContractDescriptor).GenerateClass(
                g =>
                {
                    WriteLine(
                        "public {0}({1} contractDefinition) : this(contractDefinition, {2}.Default)",
                        ContractDescriptor.Name,
                        FormatType<ContractDefinition>(),
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name);
                    using (WithBlock())
                    {
                    }

                    WriteLine();

                    WriteLine(
                        "public {0}({1} contractDefinition,{2} descriptor) : base(contractDefinition, descriptor)",
                        ContractDescriptor.Name,
                        FormatType<ContractDefinition>(),
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name);
                    using (WithBlock())
                    {
                    }
                });
        }
    }
}