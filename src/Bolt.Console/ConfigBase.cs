using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Bolt.Generators;

using Newtonsoft.Json;

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
            string output = Output;
            if (string.IsNullOrEmpty(output))
            {
                output = Path.Combine(execution.OutputDirectory, GetFileName(execution.Definition));
            }

            DocumentGenerator document = Parent.Parent.GetDocument(output);
            DoExecute(document, CoerceDescriptor(execution.Definition));
        }

        protected abstract void DoExecute(DocumentGenerator generator, ContractDefinition definition);

        protected abstract string GetFileName(ContractDefinition definition);

        private ContractDefinition CoerceDescriptor(ContractDefinition definition)
        {
            return new ContractDefinition(definition.Root, definition.ExcludedContracts.Concat(GetExcludedTypes()).Distinct().ToArray());
        }

        private IEnumerable<Type> GetExcludedTypes()
        {
            if (Excluded == null)
            {
                return Enumerable.Empty<Type>();
            }

            return Excluded.Select(TypeHelper.GetTypeOrThrow);
        }
    }
}