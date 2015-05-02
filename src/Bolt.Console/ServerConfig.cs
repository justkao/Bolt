using Bolt.Generators;

namespace Bolt.Console
{
    public class ServerConfig : ConfigBase
    {
        public ServerConfig()
        {
            GenerateExtensions = true;
        }

        public string StateFullBase { get; set; }

        public string GeneratorEx { get; set; }

        public bool Descriptor { get; set; }

        public bool GenerateExtensions { get; set; }

        protected internal override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            if (Descriptor)
            {
                IncludeDescriptors(generator, definition);
            }

            ContractActionsGenerator contractActionsGenerator = new ContractActionsGenerator
                                                  {
                                                      ContractDefinition = definition,
                                                      Namespace = Namespace,
                                                      Name = Name,
                                                      StateFullInstanceProviderBase = StateFullBase,
                                                      Modifier = GetModifier(),
                                                      GenerateExtensions = GenerateExtensions
                                                  };

            if (!string.IsNullOrEmpty(Suffix))
            {
                contractActionsGenerator.Suffix = Suffix;
            }

            if (!string.IsNullOrEmpty(Generator))
            {
                contractActionsGenerator.InvocatorUserGenerator = Parent.Parent.GetGenerator(Generator);
            }

            if (!string.IsNullOrEmpty(GeneratorEx))
            {
                contractActionsGenerator.ExtensionCodeGenerator = Parent.Parent.GetGenerator(GeneratorEx);
            }

            generator.Add(contractActionsGenerator);
        }

        public override string GetFileName(ContractDefinition definition)
        {
            return $"{definition.Name}.Server.Designer.cs";
        }
    }
}