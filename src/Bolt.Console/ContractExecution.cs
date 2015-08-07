using Bolt.Generators;

namespace Bolt.Console
{
    public class ContractExecution
    {
        public ContractExecution(ContractDefinition definition)
        {
            Definition = definition;
        }

        public ContractDefinition Definition { get; }

        public string GetOutput(ConfigBase config)
        {
            string output = PathHelpers.GetOutput(config.Parent.OutputDirectory, config.Output, config.GetFileName(Definition));
            return output;
        }
    }
}