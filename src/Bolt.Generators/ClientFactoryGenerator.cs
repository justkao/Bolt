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
                        "public {0}() : this({1}.Default)",
                        ContractDescriptor.Name,
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name);
                    using (WithBlock())
                    {
                    }

                    WriteLine();

                    WriteLine(
                        "public {0}({1} descriptor) : base(descriptor)",
                        ContractDescriptor.Name,
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name);
                    using (WithBlock())
                    {
                    }
                });
        }
    }
}