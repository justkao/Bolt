using Microsoft.Framework.Runtime;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Console
{
    public class Program
    {
        private readonly IServiceProvider _hostServices;
        private readonly IAssemblyLoadContext _loadContext;

        public Program(IServiceProvider services)
        {
            _hostServices = services;
            _loadContext = ((IAssemblyLoadContextFactory)_hostServices.GetService(typeof(IAssemblyLoadContextFactory))).Create();
        }

        public int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "bolt";

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            app.Command("example", c =>
            {
                c.Description = "Generates Bolt configuration example file.";
                var argRoot = c.Argument("[output]", "The output file path. If no value is specified Bolt.Example.json file is generated in current directory.");
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    string result = CreateSampleConfiguration(_loadContext).Serialize();
                    string outputFile = PathHelpers.GetOutput(Directory.GetCurrentDirectory(), argRoot.Value, "Bolt.Example.json");
                    File.WriteAllText(outputFile, result);

                    System.Console.WriteLine("Example configuration file created: {0}", outputFile);
                    System.Console.WriteLine();

                    return 0;
                });
            });

            app.Command("createConfig", c =>
            {
                c.Description = "Generates Configuration.json file for all interfaces in defined assembly.";
                var input = c.Argument("[input]", "Path to assembly with interface for which the Bolt configuration will be generated.");
                var output = c.Option("--output <PATH>", "Output directory or file that will be used to write Bolt configuration. If no path is specified the 'Configuration.json' file will be generated in current directory.", CommandOptionType.SingleValue);

                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    if (!File.Exists(input.Value))
                    {
                        System.Console.WriteLine("Assembly must be specified.");
                        return 1;
                    }

                    string json = RootConfig.Create(_loadContext, input.Value).Serialize();
                    var outputFile = PathHelpers.GetOutput(Path.GetDirectoryName(input.Value), output.Value(), "Configuration.json");
                    File.WriteAllText(outputFile, json);
                    System.Console.WriteLine("Configuration file created: {0}", outputFile);
                    System.Console.WriteLine();

                    return 0;
                });
            });

            app.Command("fromAssembly", c =>
            {
                c.Description = "Generate Bolt contract configuration for all interfaces that are defined in assembly.";
                var input = c.Argument("[input]", "Path to assembly with interfaces for which the Bolt code will be generated.");
                var output = c.Option("--output <DIRECTORY>", "Directory where the Bolt code will be generated. If not specified directory of input file will be used.", CommandOptionType.SingleValue);
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    if (!File.Exists(input.Value))
                    {
                        System.Console.WriteLine("Assembly must be specified.");
                        return 1;
                    }

                    RootConfig rootConfig = RootConfig.Create(_loadContext, input.Value);
                    if (output.HasValue())
                    {
                        rootConfig.OutputDirectory = output.Value();
                    }
                    else
                    {
                        rootConfig.OutputDirectory = Path.GetDirectoryName(input.Value);
                    }

                    return rootConfig.Generate();
                });
            });

            app.Command("fromConfig", c =>
            {
                c.Description = "Generates Bolt code for contracts defined in configuration file.";
                var input = c.Argument("[input]", "Path to Bolt configuration file with contract definitions.");
                var output = c.Option("--output <DIRECTORY>", "Directory where the Bolt code will be generated. If not specified directory of input file will be used.", CommandOptionType.SingleValue);
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    if (!File.Exists(input.Value))
                    {
                        System.Console.WriteLine("Bolt Configuration must be specified.");
                        return 1;
                    }

                    RootConfig rootConfig = RootConfig.Load(_loadContext, input.Value);
                    if ( output.HasValue())
                    {
                        rootConfig.OutputDirectory = output.Value();
                    }
                    else
                    {
                        rootConfig.OutputDirectory = Path.GetDirectoryName(input.Value);
                    }

                    return rootConfig.Generate();
                });
            });

            return app.Execute(args);
        }

        private static RootConfig CreateSampleConfiguration(IAssemblyLoadContext loader)
        {
            RootConfig rootConfig = new RootConfig(loader);
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
