using System;
using System.Collections.Generic;
using System.Linq;

using Bolt.Generators;

using Newtonsoft.Json;

namespace Bolt.Console
{
    public class ContractConfig
    {
        [JsonIgnore]
        private ContractDefinition _contractDefinition;

        [JsonProperty(Required = Required.Always)]
        public string Contract { get; set; }

        public List<string> Excluded { get; set; }

        public ClientConfig Client { get; set; }

        public ServerConfig Server { get; set; }

        public DescriptorConfig Descriptor { get; set; }

        public string Modifier { get; set; }

        public string Context { get; set; }

        public string GetModifier()
        {
            if (!string.IsNullOrEmpty(Modifier))
            {
                return Modifier;
            }

            if (!string.IsNullOrEmpty(Parent.Modifier))
            {
                return Parent.Modifier;
            }

            return "public";
        }

        [JsonIgnore]
        public RootConfig Parent { get; set; }

        [JsonIgnore]
        public ContractDefinition ContractDefinition
        {
            get
            {
                if (_contractDefinition == null)
                {
                    _contractDefinition = GetContractDefinition();
                }

                return _contractDefinition;
            }
        }

        public void Generate()
        {
            if (Client != null)
            {
                Client.Parent = this;
            }

            if (Server != null)
            {
                Server.Parent = this;
            }

            if (Descriptor != null)
            {
                Descriptor.Parent = this;
            }

            ContractExecution execution = new ContractExecution(ContractDefinition);
            if (Descriptor != null)
            {
                Descriptor.Execute(execution);
            }
            if (Client != null)
            {
                Client.Execute(execution);
            }
            if (Server != null)
            {
                Server.Execute(execution);
            }
        }

        private ContractDefinition GetContractDefinition()
        {
            Type type = Parent.AssemblyCache.GetType(Contract);
            List<Type> excluded = new List<Type>();

            if (Excluded != null && Excluded.Any())
            {
                excluded = Excluded.Select(Parent.AssemblyCache.GetType).ToList();
            }

            return new ContractDefinition(type, excluded.ToArray());
        }

    }
}
