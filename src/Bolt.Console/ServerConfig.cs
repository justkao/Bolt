using Bolt.Generators;

namespace Bolt.Console
{
    public class ServerConfig : ConfigBase
    {
        public string StateFullBase { get; set; }

        public string GeneratorEx { get; set; }

        public bool UseAsp { get; set; }

        public bool Descriptor { get; set; }

        protected internal override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            if (Descriptor)
            {
                IncludeDescriptors(generator, definition);
            }

            ServerGenerator serverGenerator = new ServerGenerator
                                                  {
                                                      ContractDefinition = definition,
                                                      Namespace = Namespace,
                                                      Name = Name,
                                                      StateFullInstanceProviderBase = StateFullBase,
                                                      Modifier = GetModifier(),
                                                      UseAsp = UseAsp
                                                  };

            if (!string.IsNullOrEmpty(Suffix))
            {
                serverGenerator.Suffix = Suffix;
            }

            if (!string.IsNullOrEmpty(Generator))
            {
                serverGenerator.InvocatorUserGenerator = Parent.Parent.GetGenerator(Generator);
            }

            if (!string.IsNullOrEmpty(GeneratorEx))
            {
                serverGenerator.ExtensionCodeGenerator = Parent.Parent.GetGenerator(GeneratorEx);
            }

            generator.Add(serverGenerator);
        }

        public override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Server.Designer.cs", definition.Name);
        }
    }
}