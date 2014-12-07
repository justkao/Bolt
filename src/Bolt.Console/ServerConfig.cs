using Bolt.Generators;

namespace Bolt.Console
{
    public class ServerConfig : ConfigBase
    {
        public string Namespace { get; set; }

        public string Suffix { get; set; }

        public string Name { get; set; }

        public string Modifier { get; set; }

        public string StateFullBase { get; set; }

        public string Generator { get; set; }

        public string GeneratorEx { get; set; }

        protected override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ServerGenerator serverGenerator = new ServerGenerator
                                                  {
                                                      ContractDefinition = definition,
                                                      Namespace = Namespace,
                                                      Name = Name,
                                                      StateFullInstanceProviderBase = StateFullBase,
                                                      Modifier = Modifier ?? "public"
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

        protected override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Server.Designer.cs", definition.Name);
        }
    }
}