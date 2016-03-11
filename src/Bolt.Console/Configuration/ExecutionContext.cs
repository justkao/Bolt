using Bolt.Console.Generators;

namespace Bolt.Console.Configuration
{
    public class ExecutionContext
    {
        public ExecutionContext(ContractDefinition definition)
        {
            Definition = definition;
        }

        public ContractDefinition Definition { get; }

        public string GetOutput(ConfigurationBase configuration)
        {
            string output = PathHelpers.GetOutput(configuration.Parent.OutputDirectory, configuration.Output, configuration.GetFileName(Definition));
            return output;
        }
    }
}