using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Console
{
    public class Program
    {
        private enum BoltAction
        {
            GenerateCode,
            GenerateCodeFromAssembly,
            Help,
            GenerateConfig,
            GenerateExample
        }

        public static int Main(string[] args)
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
                    "root=", "The working directory for assembly loading.",
                    v => workingDirectory = v
                },
                {
                    "example", "Generates configuration example.", v =>
                    {
                        action = BoltAction.GenerateExample;
                    }
                },
                {
                    "fromConfig=", "The path to configuration file.",
                    v =>
                    {
                        inputPath = v;
                        action = BoltAction.GenerateCode;
                    }
                },
                {
                    "createConfig=", "Generates Configuration.json file for all interfaces in defined assembly.",
                    v =>
                    {
                        inputPath = v;
                        action = BoltAction.GenerateConfig;
                    }
                },
                {
                    "fromAssembly=", "Generates Bolt code from interfaces defined in assembly.",
                    v =>
                    {
                        inputPath = v;
                        action = BoltAction.GenerateCodeFromAssembly;
                    }
                },
                {
                    "output=", "Defines output file or directory.",
                    v =>
                    {
                        outputPath = v;
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
                return 0;
            }

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }
            string outputFile = null;

            try
            {
                switch (action)
                {
                    case BoltAction.GenerateCode:
                        EnsureInput(inputPath, set);
                        RootConfig rootConfig = RootConfig.Load(inputPath);
                        rootConfig.OutputDirectory = outputPath ?? Path.GetDirectoryName(inputPath);
                        rootConfig.Generate();
                        break;
                    case BoltAction.Help:
                        ShowHelp(set);
                        break;
                    case BoltAction.GenerateCodeFromAssembly:
                        EnsureInput(inputPath, set);
                        RootConfig config = RootConfig.Create(inputPath);
                        config.OutputDirectory = outputPath ?? Path.GetDirectoryName(inputPath);
                        config.Generate();
                        break;
                    case BoltAction.GenerateConfig:
                        EnsureInput(inputPath, set);
                        string json = RootConfig.Create(inputPath).Serialize();
                        outputFile = PathHelpers.GetOutput(Path.GetDirectoryName(inputPath), outputPath, "Configuration.json");
                        File.WriteAllText(outputFile, json);
                        break;
                    case BoltAction.GenerateExample:
                        string result = CreateSampleConfiguration().Serialize();
                        outputFile = PathHelpers.GetOutput(Environment.CurrentDirectory, outputPath, "Bolt.Example.json");
                        File.WriteAllText(outputFile, result);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                ConsoleColor prev = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Failed to execute '{0}' action.", action);
                System.Console.WriteLine("Error: '{0}'", e.Message);
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.WriteLine(e.ToString());
                System.Console.WriteLine(e.ToString());
                System.Console.ForegroundColor = prev;
                return 1;
            }

            return 0;
        }

        private static void EnsureInput(string inputPath, OptionSet set)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                System.Console.Error.WriteLine("The input must be specified.");
                set.WriteOptionDescriptions(System.Console.Error);
                Environment.Exit(-1);
            }

            if (!File.Exists(inputPath))
            {
                System.Console.Error.WriteLine("The file '{0}' does not exist.", inputPath);
                set.WriteOptionDescriptions(System.Console.Error);
                Environment.Exit(-1);
            }
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
            rootConfig.Modifier = "<public|internal>";
            rootConfig.Assemblies = new List<string>() { "<AssemblyPath>", "<AssemblyPath>", "<AssemblyPath>" };
            rootConfig.Generators = new List<GeneratorConfig>()
                                        {
                                            new GeneratorConfig()
                                                {
                                                    Name = "<GeneratorName>",
                                                    Type = "<FullTypeName>",
                                                    Properties = new Dictionary<string, string>()
                                                                     {
                                                                         { "<Name>", "<Value>" }
                                                                     }
                                                }
                                        };

            rootConfig.Contracts = new List<ContractConfig>();
            rootConfig.Contracts.Add(new ContractConfig()
            {
                Contract = "<Type>",
                Modifier = "<public|internal>",
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
                    Output = "<Path>",
                    Excluded = new List<string>() { "<FullTypeName>" },
                    Suffix = "<Suffix> // suffix for generated server invokers, defaults to 'Invoker'",
                    Modifier = "<public|internal>",
                    Namespace = "<Namespace> // namespace of generated server invoker, defaults to contract namespace if null",
                    Name = "<ProxyName> // name of generated server invoker, defaults to 'ContractName + Suffix' if null",
                    Generator = "<GeneratorName> // user defined generator for server invokers",
                    GeneratorEx = "<GeneratorName> // user defined generator for invoker extensions",
                    StateFullBase = "<FullTypeName> // base class used for statefull invokers"
                },
                Descriptor = new DescriptorConfig()
                {
                    Modifier = "<public|internal>",
                    Output = "<Path>",
                }
            });

            return rootConfig;
        }
    }
}
