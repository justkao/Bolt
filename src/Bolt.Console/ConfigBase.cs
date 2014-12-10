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

        public void Execute(ContractExecution execution)
        {
            string output = PathHelpers.GetOutput(execution.OutputDirectory, Output, GetFileName(execution.Definition));
            DocumentGenerator document = Parent.Parent.GetDocument(output);
            document.Context = Parent.Context;
            document.Formatter.Assemblies.AddRange(Parent.Parent.AssemblyCache);
            DoExecute(document, CoerceDescriptor(execution.Definition));
        }

        protected abstract void DoExecute(DocumentGenerator generator, ContractDefinition definition);

        protected abstract string GetFileName(ContractDefinition definition);

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
    }
}