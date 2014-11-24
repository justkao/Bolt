using Bolt.Generators;

namespace Bolt.Console
{
    public class ContractExecution
    {
        public ContractExecution(ContractDefinition definition, string outputDirectory)
        {
            Definition = definition;
            OutputDirectory = outputDirectory;
        }

        public ContractDefinition Definition { get; private set; }

        public string OutputDirectory { get; private set; }
    }
}