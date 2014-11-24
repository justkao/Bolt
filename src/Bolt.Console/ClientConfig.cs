using Bolt.Client;
using Bolt.Generators;

namespace Bolt.Console
{
    public class ClientConfig : ConfigBase
    {
        public bool ForceAsync { get; set; }

        public bool GenerateFactory { get; set; }

        public bool CustomBaseClass { get; set; }

        public string Namespace { get; set; }

        public string Suffix { get; set; }

        protected override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ClientGenerator clientGenerator = new ClientGenerator()
                                {
                                    ForceAsync = ForceAsync,
                                    ContractDefinition = definition,
                                    GenerateFactory = GenerateFactory,
                                    Namespace = Namespace
                                };

            if (CustomBaseClass)
            {
                clientGenerator.BaseClass = clientGenerator.FormatType<IChannel>();
            }

            if (!string.IsNullOrEmpty(Suffix))
            {
                clientGenerator.Suffix = Suffix;
            }

            if (ForceAsync)
            {
                generator.Add(new InterfaceGenerator()
                                  {
                                      ContractDefinition = definition,
                                      ForceAsync = ForceAsync
                                  });
            }


            generator.Add(clientGenerator);
        }

        protected override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Client.Designer.cs", definition.Name);
        }
    }
}