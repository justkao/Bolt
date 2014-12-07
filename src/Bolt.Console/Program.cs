using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace Bolt.Console
{
    public class Program
    {
        private enum BoltAction
        {
            GenerateCode,
            Help,
            GenerateConfig,
            GenerateExample
        }

        public static void Main(string[] args)
        {
            BoltAction action = BoltAction.Help;
            string workingDirectory = null;
            string inputPath = null;
            string outputPath = null;

            OptionSet set = new OptionSet()
            {
                {
                    "h|help", "Shows help for bolt tool.",
                    (v) => { action = BoltAction.Help; }
                },
                {
                    "r:|root:", "The working directory for assembly loading.",
                    v => workingDirectory = v
                },
                {
                    "e|example", "Shows or generates configuration example.", v =>
                    {
                        action = BoltAction.GenerateExample;
                    }
                },
                {
                    "c=|config=", "The path to configuration file.",
                    v =>
                    {
                        inputPath = v;
                        action = BoltAction.GenerateCode;
                    }
                },
                {
                    "g=|generate=", "Generates Configuration.json file for all interfaces in defined assembly.",
                    v =>
                    {
                        inputPath = v;
                        action = BoltAction.GenerateConfig;
                    }
                },
                {
                    "o=|output=", "Defines output file or directory.",
                    v =>
                    {
                        outputPath = v;
                        action = BoltAction.GenerateConfig;
                    }
                },
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

            switch (action)
            {
                case BoltAction.GenerateCode:
                    if (string.IsNullOrEmpty(inputPath))
                    {
                        System.Console.Error.WriteLine("The configuration path must be specified.");
                        set.WriteOptionDescriptions(System.Console.Error);
                        Environment.Exit(-1);
                    }

                    if (!string.IsNullOrEmpty(workingDirectory))
                    {
                        Directory.SetCurrentDirectory(workingDirectory);
                    }

                    RootConfig rootConfig = RootConfig.Load(inputPath);
                    rootConfig.OutputDirectory = Path.GetDirectoryName(inputPath);
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        rootConfig.OutputDirectory = Path.GetDirectoryName(outputPath);
                    }
                    rootConfig.Generate();
                    break;
                case BoltAction.Help:
                    ShowHelp(set);
                    break;
                case BoltAction.GenerateConfig:
                    string json = RootConfig.Create(inputPath).Serialize();
                    File.WriteAllText(outputPath ?? Path.Combine(Path.GetDirectoryName(inputPath), "Configuration.json"), json);
                    break;
                case BoltAction.GenerateExample:
                    string result = CreateSampleConfiguration().Serialize();
                    File.WriteAllText(outputPath ?? "Bolt.Example.json", result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Environment.Exit(0);
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
                Modifier = "<public|internal>",
                Output = "<Path>",
                Context = "<Context> // passed to user code generators",
                Excluded = new List<string> { "<FullTypeName>", "<FullTypeName>" },
                Client = new ClientConfig()
                {
                    ForceAsync = true,
                    Generator = "<GeneratorName>",
                    Output = "<Path>",
                    Excluded = new List<string>() { "<FullTypeName>", "<FullTypeName>" },
                    Suffix = "<Suffix> // suffix for generated client proxy, defaults to 'Proxy'",
                    Modifier = "<public|internal>",
                    Namespace = "<Namespace> // namespace of generated proxy, defaults to contract namespace if null",
                    Name = "<ProxyName> // name of generated proxy, defaults to 'ContractName + Suffix' if null",
                    ExcludedInterfaces = new List<string>() { "<FullTypeName>", "<FullTypeName>" }
                },
                Server = new ServerConfig()
                {
                    ForceAsync = true,
                    Output = "<Path>",
                    Excluded = new List<string>() { "<FullTypeName>" },
                    Suffix = "<Suffix> // suffix for generated server invokers, defaults to 'Invoker'",
                    Modifier = "<public|internal>",
                    Namespace = "<Namespace> // namespace of generated server invoker, defaults to contract namespace if null",
                    Name = "<ProxyName> // name of generated server invoker, defaults to 'ContractName + Suffix' if null",
                    Generator = "<GeneratorName> // user defined generator for server invokers",
                    GeneratorEx = "<GeneratorName> // user defined generator for invoker extensions",
                    StateFullBase = "<FullTypeName> // base class used for statefull invokers"
                }
            });

            return rootConfig;
        }
    }
}
