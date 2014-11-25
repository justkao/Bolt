using Bolt.Generators;
using System.Linq;

namespace Bolt.Console
{
    public class ClientConfig : ConfigBase
    {
        public bool ForceAsync { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Suffix { get; set; }

        protected override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ClientGenerator clientGenerator = new ClientGenerator()
            {
                ForceAsync = ForceAsync,
                ContractDefinition = definition,
                Namespace = Namespace,
                Name = Name
            };

            InterfaceGenerator interfaceGenerator = new InterfaceGenerator()
            {
                ContractDefinition = definition,
                ForceAsync = ForceAsync
            };

            generator.Add(interfaceGenerator);
            interfaceGenerator.Generated += (s, e) =>
            {
                clientGenerator.BaseInterfaces = interfaceGenerator.GeneratedAsyncInterfaces.ToList();
            };

            if (!string.IsNullOrEmpty(Suffix))
            {
                clientGenerator.Suffix = Suffix;
            }

            generator.Add(clientGenerator);
        }

        protected override string GetFileName(ContractDefinition definition)
        {
            return string.Format("{0}.Client.Designer.cs", definition.Name);
        }
    }
}