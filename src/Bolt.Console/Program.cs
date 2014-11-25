using System.Collections.Generic;
using System.IO;

namespace Bolt.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Tool used to generate Bolt client and server classes.");
                System.Console.WriteLine();
                System.Console.WriteLine("Usage:");
                System.Console.WriteLine("      Bolt -example <output>   : Generates configuration example at defined path.");
                System.Console.WriteLine("      Bolt <config>            : Generate Bolt classes based on provided configuration file.");

                return;
            }

            if (args[0] == "-example")
            {
                string result = CreateSampleConfiguration().Serialize();
                string output = "Bolt.Example.json";

                if (args.Length > 1)
                {
                    output = args[1];
                }

                File.WriteAllText(output, result);
                return;
            }

            RootConfig config = RootConfig.Load(args[0]);
            config.OutputDirectory = Path.GetDirectoryName(args[0]);
            config.Generate();
        }

        private static RootConfig CreateSampleConfiguration()
        {
            RootConfig rootConfig = new RootConfig();
            rootConfig.Contracts = new List<ContractConfig>();
            rootConfig.Contracts.Add(new ContractConfig()
            {
                Assemblies = new List<string>() { "<AssemblyPath>" },
                Contract = "<Type>",
                Output = "<Directory or File Path>",
                Excluded = new List<string> { "<ExcludedType1>", "<ExcludedType2>" },
                ParametersBase = "<Base Class For Generated Parameters or null>",
                Client = new ClientConfig()
                {
                    ForceAsync = true,
                    Output = "<Directory or File Path>",
                    Excluded = new List<string>() { "<Additional Excluded Type >" },
                    Suffix = "<Generated Client Classes Suffix>"
                }
            });

            return rootConfig;
        }
    }
}
