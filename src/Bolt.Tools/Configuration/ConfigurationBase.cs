using System;
using System.Collections.Generic;
using System.Linq;
using Bolt.Tools.Generators;
using Newtonsoft.Json;

namespace Bolt.Tools.Configuration
{
    public abstract class ConfigurationBase
    {
        [JsonIgnore]
        public RootConfiguration Parent { get; set; }

        public string Output { get; set; }

        public List<string> Excluded { get; set; }

        public string Modifier { get; set; }

        public string Namespace { get; set; }

        public string Suffix { get; set; }

        public string Name { get; set; }

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

            if (!string.IsNullOrEmpty(Parent.Modifier))
            {
                return Parent.Modifier;
            }

            return "public";
        }

        public DocumentGenerator Prepare(ExecutionContext executionContext)
        {
            DocumentGenerator document = Parent.GetDocument(executionContext.GetOutput(this));
            document.Formatter.Assemblies.AddRange(Parent.AssemblyCache.Loader);
            DoPrepare(document, CoerceContractDefinition(executionContext.Definition));
            return document;
        }

        public abstract string GetFileName(ContractDefinition definition);

        protected internal abstract void DoPrepare(DocumentGenerator generator, ContractDefinition definition);

        private ContractDefinition CoerceContractDefinition(ContractDefinition definition)
        {
            return new ContractDefinition(definition.Root, definition.ExcludedContracts.Concat(GetExcludedTypes()).Distinct().ToArray());
        }

        private IEnumerable<Type> GetExcludedTypes()
        {
            if (Excluded == null)
            {
                return Enumerable.Empty<Type>();
            }

            return Excluded.Select(Parent.AssemblyCache.GetType);
        }
    }
}