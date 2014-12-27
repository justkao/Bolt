using Bolt.Generators;

namespace Bolt.Console
{
    public class DescriptorConfig : ConfigBase
    {
        protected internal override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            generator.Add(new ContractGenerator
            {
                ContractDefinition = definition,
                Modifier = GetModifier(),
            });

            generator.Add(new ContractDescriptorGenerator()
            {
                ContractDefinition = definition,
                Modifier = GetModifier()
            });
        }

        public override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Contract.Designer.cs", definition.Name);
        }
    }
}