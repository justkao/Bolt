using System.Collections.Generic;
using System.Linq;
using Bolt.Generators;

namespace Bolt.Console
{
    public class ClientConfig : ConfigBase
    {
        public bool ForceAsync { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Suffix { get; set; }

        public List<string> ExcludedInterfaces { get; set; }

        public string Modifier { get; set; }

        protected override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ClientGenerator clientGenerator = new ClientGenerator()
            {
                ForceAsync = ForceAsync,
                ContractDefinition = definition,
                Namespace = Namespace,
                Name = Name
            };

            if (!string.IsNullOrEmpty(Modifier))
            {
                clientGenerator.Modifier = Modifier;
            }

            InterfaceGenerator interfaceGenerator = new InterfaceGenerator()
            {
                ContractDefinition = definition,
                ForceAsync = ForceAsync,
                ExcludedInterfaces = ExcludedInterfaces
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