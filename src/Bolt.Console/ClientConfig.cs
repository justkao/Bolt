using System.Collections.Generic;
using System.Linq;
using Bolt.Generators;

namespace Bolt.Console
{
    public class ClientConfig : ConfigBase
    {
        public bool ForceAsync { get; set; }

        public List<string> ExcludedInterfaces { get; set; }

        public bool Descriptor { get; set; }

        protected internal override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            if (Descriptor)
            {
                IncludeDescriptors(generator, definition);
            }

            ClientGenerator clientGenerator = new ClientGenerator
            {
                ForceAsync = ForceAsync,
                ContractDefinition = definition,
                Namespace = Namespace,
                Name = Name,
                Modifier = GetModifier()
            };

            if (!string.IsNullOrEmpty(Generator))
            {
                clientGenerator.UserGenerator = Parent.Parent.GetGenerator(Generator);
            }

            InterfaceGenerator interfaceGenerator = new InterfaceGenerator
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

        public override string GetFileName(ContractDefinition definition)
        {
            return $"{definition.Name}.Client.Designer.cs";
        }
    }
}