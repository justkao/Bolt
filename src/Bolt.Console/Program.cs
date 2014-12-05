using System;
using System.Collections.Generic;
using System.IO;

using NDesk.Options;

namespace Bolt.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool showExample = false;
            string examplePath = null;
            string workingDirectory = null;
            string config = null;

            OptionSet set = new OptionSet()
                                {
                                    {
                                        "h|help", "Shows help for bolt tool.", 
                                        v => showHelp = v != null
                                    },
                                    {
                                        "r=|root=", "The working directory for assembly loading.",
                                        v => workingDirectory = v
                                    },
                                    {
                                        "e|example:", "Shows or generates configuration example.", v =>
                                            {
                                                showExample = true;
                                                examplePath = v;
                                            }
                                    },
                                    {
                                        "c=|config=", "The path to configuration file.",
                                        v => config = v
                                    }
                                };

            try
            {
                set.Parse(args);
            }
            catch (OptionException e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Try 'Bolt --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(set);
                return;
            }

            if (showExample)
            {
                string result = CreateSampleConfiguration().Serialize();
                File.WriteAllText(examplePath ?? "Bolt.Example.json", result);
                return;
            }

            if (string.IsNullOrEmpty(config))
            {
                System.Console.Error.WriteLine("The configuration path must be specified.");
                set.WriteOptionDescriptions(System.Console.Error);
                Environment.Exit(-1);
            }

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }

            RootConfig rootConfig = RootConfig.Load(config);
            rootConfig.OutputDirectory = Path.GetDirectoryName(config);
            rootConfig.Generate();
        }

        private static void ShowHelp(OptionSet p)
        {
            System.Console.WriteLine("Usage: Bolt [OPTIONS]");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            p.WriteOptionDescriptions(System.Console.Out);
        }

        private static RootConfig CreateSampleConfiguration()
        {
            RootConfig rootConfig = new RootConfig();
            rootConfig.Assemblies = new List<string>() { "<AssemblyPath>", "<AssemblyPath>", "<AssemblyPath>" };
            rootConfig.Generators = new List<GeneratorConfig>()
                                        {
                                            new GeneratorConfig()
                                                {
                                                    Name = "<GeneratorName>",
                                                    Type = "<FullTypeName>"
                                                }
                                        };

            rootConfig.Contracts = new List<ContractConfig>();
            rootConfig.Contracts.Add(new ContractConfig()
            {
                Contract = "<Type>",
                Modifier = "public",
                Output = "<Directory or File Path>",
                Excluded = new List<string> { "<ExcludedType1>", "<ExcludedType2>" },
                Client = new ClientConfig()
                {
                    ForceAsync = true,
                    Generator = "<GeneratorName>",
                    Output = "<Directory or File Path>",
                    Excluded = new List<string>() { "<Additional Excluded Type >" },
                    Suffix = "<Generated Client Classes Suffix>",
                    Modifier = "public",
                    Namespace = "Client.Proxy.Namespace",
                    Name = "ProxyName",
                    ExcludedInterfaces = new List<string>() { "<Interface that will be excluded from Async proxy generation>" },
                },
                Server = new ServerConfig()
                {
                    ForceAsync = true,
                    Output = "<Directory or File Path>",
                    Excluded = new List<string>() { "<Additional Excluded Type >" },
                    Suffix = "<Generated Server Classes Suffix>",
                    Modifier = "public",
                    Namespace = "Server.Invoker.Namespace",
                    Name = "ServerInvokerName",
                    Generator = "<GeneratorName>",
                    GeneratorEx = "<GeneratorName>",
                    StateFullBase = "<base class for statefull instance provider>"
                }
            });

            return rootConfig;
        }
    }
}
