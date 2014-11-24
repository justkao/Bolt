using Bolt.Generators;

namespace Bolt.Console
{
    public class ServerConfig : ConfigBase
    {
        public bool ForceAsync { get; set; }

        public bool GenerateFactory { get; set; }

        public bool CustomBaseClass { get; set; }

        public string Namespace { get; set; }

        public string Suffix { get; set; }

        protected override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ServerGenerator serverGenerator = new ServerGenerator { ContractDefinition = definition, Namespace = Namespace };

            if (CustomBaseClass)
            {
                serverGenerator.BaseClass = null;
            }

            if (!string.IsNullOrEmpty(Suffix))
            {
                serverGenerator.Suffix = Suffix;
            }

            generator.Add(serverGenerator);
        }

        protected override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Server.Designer.cs", definition.Name);
        }
    }
}