using System;
using System.Collections.Generic;
using System.Linq;
using Bolt.Console.Generators;
using Newtonsoft.Json;

namespace Bolt.Console.Configuration
{
    public class InterfaceConfiguration : ConfigurationBase
    {
        [JsonIgnore]
        private ContractDefinition _contractDefinition;

        [JsonProperty(Required = Required.Always)]
        public string Contract { get; set; }

        public bool ForceAsync { get; set; }

        public bool ForceSync { get; set; }

        public List<string> ExcludedInterfaces { get; set; }

        protected internal override void DoPrepare(DocumentGenerator generator, ContractDefinition definition)
        {
            InterfaceGenerator interfaceGenerator = new InterfaceGenerator
            {
                ContractDefinition = definition,
                ForceAsync = ForceAsync,
                ForceSync = ForceSync,
                InterfaceSuffix = Suffix,
                ExcludedInterfaces = ExcludedInterfaces,
                Name = Name,
                Modifier = Modifier
            };

            if (!string.IsNullOrEmpty(Suffix))
            {
                interfaceGenerator.InterfaceSuffix = Suffix;
            }

            generator.Add(interfaceGenerator);
        }

        public override string GetFileName(ContractDefinition definition)
        {
            return $"{definition.Name}.Designer.cs";
        }

        public InterfaceConfiguration AddExcluded(List<string> excludedContracts)
        {
            if (excludedContracts == null || !excludedContracts.Any())
            {
                return this;
            }

            List<Type> excluded = excludedContracts.Select(c =>
            {
                try
                {
                    return Parent.AssemblyCache.GetType(c);
                }
                catch (Exception)
                {
                    return null;
                }
            }).Where(t => t != null).ToList();

            if (excluded.Any())
            {
                if (ExcludedInterfaces == null)
                {
                    ExcludedInterfaces = new List<string>();
                }

                ExcludedInterfaces.AddRange(excluded.Select(c => c.FullName));
            }

            return this;
        }

        [JsonIgnore]
        public ContractDefinition ContractDefinition => _contractDefinition ?? (_contractDefinition = GetContractDefinition());

        public void Generate()
        {
            ExecutionContext executionContext = new ExecutionContext(ContractDefinition);
            Prepare(executionContext).Generate(null);
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
