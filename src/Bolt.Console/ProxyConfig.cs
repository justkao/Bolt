using System;
using System.Collections.Generic;
using System.Linq;
using Bolt.Generators;
using Newtonsoft.Json;

namespace Bolt.Console
{
    public class ProxyConfig : ConfigBase
    {
        public ProxyConfig()
        {
            Mode = GenerateContractMode.All;
        }

        [JsonIgnore]
        private ContractDefinition _contractDefinition;

        [JsonProperty(Required = Required.Always)]
        public string Contract { get; set; }

        public bool ForceAsync { get; set; }

        public bool ForceSync { get; set; }

        public List<string> ExcludedInterfaces { get; set; }

        public GenerateContractMode Mode { get; set; }

        protected internal override void DoExecute(DocumentGenerator generator, ContractDefinition definition)
        {
            ProxyGenerator proxyGenerator = null;
            if (Mode.HasFlag(GenerateContractMode.Proxy))
            {
                proxyGenerator = new ProxyGenerator
                {
                    ForceAsync = ForceAsync,
                    ForceSync =  ForceSync,
                    ContractDefinition = definition,
                    Namespace = Namespace,
                    Name = Name,
                    Modifier = GetModifier()
                };

                if (!string.IsNullOrEmpty(Generator))
                {
                    proxyGenerator.UserGenerator = Parent.GetGenerator(Generator);
                }

                if (!string.IsNullOrEmpty(Suffix))
                {
                    proxyGenerator.Suffix = Suffix;
                }
            }

            if (Mode.HasFlag(GenerateContractMode.Interface))
            {
                InterfaceGenerator interfaceGenerator = new InterfaceGenerator
                {
                    ContractDefinition = definition,
                    ForceAsync = ForceAsync,
                    ForceSync = ForceSync,
                    ExcludedInterfaces = ExcludedInterfaces
                };

                generator.Add(interfaceGenerator);
                interfaceGenerator.Generated += (s, e) =>
                    {
                        if (proxyGenerator != null)
                        {
                            proxyGenerator.BaseInterfaces = interfaceGenerator.GeneratedAsyncInterfaces.ToList();
                        }
                    };
            }

            if (proxyGenerator != null)
            {
                generator.Add(proxyGenerator);
            }
        }

        public override string GetFileName(ContractDefinition definition)
        {
            return $"{definition.Name}.Proxy.Designer.cs";
        }

        [JsonIgnore]
        public ContractDefinition ContractDefinition => _contractDefinition ?? (_contractDefinition = GetContractDefinition());

        public void Generate()
        {
            ContractExecution execution = new ContractExecution(ContractDefinition);
            Execute(execution);
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
