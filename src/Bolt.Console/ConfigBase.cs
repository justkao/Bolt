using Bolt.Generators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Console
{
    public abstract class ConfigBase
    {
        [JsonIgnore]
        public ContractConfig Parent { get; set; }

        public string Output { get; set; }

        public List<string> Excluded { get; set; }

        public string Modifier { get; set; }

        public string Namespace { get; set; }

        public string Suffix { get; set; }

        public string Name { get; set; }

        public string Generator { get; set; }

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

            if (!string.IsNullOrEmpty(Parent.Parent.Modifier))
            {
                return Parent.Parent.Modifier;
            }

            return "public";
        }

        public void Execute(ContractExecution execution)
        {
            DocumentGenerator document = Parent.Parent.GetDocument(execution.GetOutput(this));
            document.Context = Parent.Context;
            document.Formatter.Assemblies.AddRange(Parent.Parent.AssemblyCache);
            DoExecute(document, CoerceDescriptor(execution.Definition));
        }

        protected internal abstract void DoExecute(DocumentGenerator generator, ContractDefinition definition);

        public abstract string GetFileName(ContractDefinition definition);

        private ContractDefinition CoerceDescriptor(ContractDefinition definition)
        {
            return new ContractDefinition(
                definition.Root,
                definition.ExcludedContracts.Concat(GetExcludedTypes()).Distinct().ToArray())
                       {
                           ParametersBase = definition.ParametersBase
                       };
        }

        private IEnumerable<Type> GetExcludedTypes()
        {
            if (Excluded == null)
            {
                return Enumerable.Empty<Type>();
            }

            return Excluded.Select(Parent.Parent.AssemblyCache.GetType);
        }

        protected void IncludeDescriptors(DocumentGenerator generator, ContractDefinition definition)
        {
            DescriptorConfig config = new DescriptorConfig();
            config.Parent = Parent;
            config.Modifier = "internal";
            config.DoExecute(generator, definition);
        }
    }
}