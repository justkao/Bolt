using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Tools.Configuration;
using Bolt.Tools.Generators;
using Microsoft.Extensions.CommandLineUtils;

namespace Bolt.Tools
{
    public static class Program
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);
        private static readonly AnsiConsole ErrorConsole = AnsiConsole.GetError(true);

        private static readonly AssemblyCache Cache = new AssemblyCache();

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication { Name = "bolt" };
            app.VersionOption("--version", GetVersion());
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            app.Command("example", c =>
            {
                c.Description = "Generates configuration file example.";
                var argRoot = c.Argument("[output]", "The output configuration file path. If no value is specified then 'bolt.example.json' file will be generated in current directory.");
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    try
                    {
                        string result = CreateSampleConfiguration(Cache).Serialize();
                        string outputFile = PathHelpers.GetOutput(Directory.GetCurrentDirectory(), argRoot.Value, "bolt.example.json");
                        bool exist = File.Exists(outputFile);
                        File.WriteAllText(outputFile, result);
                        if (exist)
                        {
                            Console.WriteLine($"Examle overwritten: {outputFile.White().Bold()}".Green().Bold());
                        }
                        else
                        {
                            Console.WriteLine($"Example created: {outputFile.White().Bold()}".Green());
                        }
                    }
                    catch(Exception e)
                    {
                        return HandleError("Failed to generate example configuration file.", e);
                    }

                    return 0;
                });
            });

            app.Command("config", c =>
            {
                c.Description = "Generates configuration file for all interfaces in assembly.";
                var input = c.Argument("[input]", "Assembly used to generate configuration file.");
                var output = c.Option("--output <PATH>", "Directory or configuration file path. If no path is specified then the 'bolt.configuration.json' configuration file will be generated in current directory.", CommandOptionType.SingleValue);

                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(input.Value))
                    {
                        Console.WriteLine("Assembly must be specified.".Yellow());
                        return 1;
                    }

                    if (!File.Exists(input.Value))
                    {
                        Console.WriteLine($"Assembly not found: {input.Value.White()}".Yellow());
                        return 1;
                    }

                    try
                    {
                        string json = RootConfiguration.CreateFromAssembly(Cache, input.Value, false).Serialize();
                        var outputFile = PathHelpers.GetOutput(Path.GetDirectoryName(input.Value), output.Value(), "bolt.configuration.json");
                        bool exist = File.Exists(outputFile);
                        File.WriteAllText(outputFile, json);
                        if (exist)
                        {
                            Console.WriteLine($"Configuration overwritten: {outputFile.White().Bold()}".Green().Bold());
                        }
                        else
                        {
                            Console.WriteLine($"Configuration created: {outputFile.White().Bold()}".Green());
                        }
                    }
                    catch (Exception e)
                    {
                        return HandleError("Failed to generate Bolt configuration file.", e);
                    }

                    return 0;
                });
            });

            app.Command("code", c =>
            {
                c.Description = "Generates code from assembly or from configuration file.";
                var input = c.Argument("[input]", "Path to the assembly or configuration file.");
                var output = c.Option("--output <DIRECTORY>", "Directory where the Bolt code will be generated. If directory is not specified then the input path directory will be used instead.", CommandOptionType.SingleValue);
                var dirOption = c.Option("--dir <PATH>", "Directories where contract assemblies are located.", CommandOptionType.MultipleValue);
                var contractOption = c.Option("--contract <NAME>", "Additional contracts to generate, if not included in config file or assembly.", CommandOptionType.MultipleValue);
                var excludedContractOption = c.Option("--excluded-contract <NAME>", "Contracts that will be exluded when generating interfaces.", CommandOptionType.MultipleValue);
                var internalSwitch = c.Option("--internal", "Generates the contracts with internal visibility.", CommandOptionType.NoValue);
                var forceAsync = c.Option("--force-async", "Generates asynchronous version of methods.", CommandOptionType.NoValue);
                var forceSync = c.Option("--force-sync", "Generates synchronous version of methods.", CommandOptionType.NoValue);
                var suffix = c.Option("--suffix", "Suffix for generated interfaces. Default value is 'Async'.", CommandOptionType.SingleValue);


                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    bool inputExists = !string.IsNullOrEmpty(input.Value);

                    if (!inputExists)
                    {
                        Console.WriteLine("Input path must be specified.".Yellow());
                        return 1;
                    }

                    var extension = inputExists ? Path.GetExtension(input.Value) : null;

                    if (!File.Exists(input.Value))
                    {
                        Console.WriteLine($"File not found: {input.Value.White().Bold()}".Yellow());
                        return 1;
                    }

                    if (inputExists)
                    {
                        Cache.Loader.AddDirectory(Path.GetDirectoryName(Path.GetFullPath(input.Value)));
                    }

                    foreach (var dir in dirOption.Values)
                    {
                        if (Directory.Exists(dir))
                        {
                            Cache.Loader.AddDirectory(dir);
                        }
                    }

                    bool asInternal = internalSwitch.HasValue();

                    RootConfiguration rootConfiguration;
                    if (inputExists)
                    {
                        if (extension == ".exe" || extension == ".dll")
                        {
                            rootConfiguration = new RootConfiguration(Cache);
                            try
                            {
                                Console.WriteLine($"Loading all contracts from assembly: ${input.Value}");
                                Cache.Loader.Load(input.Value);
                                rootConfiguration.Assemblies.Add(input.Value);
                            }
                            catch (Exception e)
                            {
                                return HandleError($"Failed to read assembly: {input.Value.White().Bold()}", e);
                            }

                            if (AddContracts(rootConfiguration, contractOption.Values, excludedContractOption.Values, asInternal, forceAsync.HasValue(), forceSync.HasValue(), suffix.Value()) != 0)
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            try
                            {
                                rootConfiguration = RootConfiguration.CreateFromConfig(Cache, input.Value);
                            }
                            catch (Exception e)
                            {
                                return HandleError($"Failed to read Bolt configuration: {input.Value.White().Bold()}", e);
                            }
                        }
                    }
                    else
                    {
                        rootConfiguration = new RootConfiguration(Cache);
                        if (AddContracts(rootConfiguration, contractOption.Values, excludedContractOption.Values, asInternal, forceAsync.HasValue(), forceSync.HasValue(), suffix.Value()) != 0)
                        {
                            return 1;
                        }
                    }

                    if (output.HasValue())
                    {
                        rootConfiguration.OutputDirectory = output.Value();
                    }
                    else
                    {
                        rootConfiguration.OutputDirectory = Path.GetDirectoryName(input.Value);
                    }

                    return rootConfiguration.Generate();
                });
            });

            return app.Execute(args);
        }

        private static int AddContracts(RootConfiguration rootConfiguration, List<string> contracts, List<string> excludedContracts, bool internalVisibility, bool forceAsync, bool forceSync, string suffix)
        {
            if (!contracts.Any() || contracts.Any(c => c.EndsWith(".*", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Adding all contracts ... ");

                try
                {
                    foreach (InterfaceConfiguration contract in rootConfiguration.AddAllContracts(internalVisibility))
                    {
                        contract.Suffix = suffix;
                        contract.ForceSync = forceSync;
                        contract.ForceAsync = forceAsync;
                        contract.AddExcluded(excludedContracts);
                    }
                }
                catch (Exception e)
                {
                    return HandleError($"Failed to resolve contracts", e);
                }

                return 0;
            }

            foreach (var contract in contracts)
            {
                if (contract.EndsWith(".*", StringComparison.OrdinalIgnoreCase))
                {
                    var ns = contract.TrimEnd('*', '.');
                    try
                    {
                        foreach (var config in rootConfiguration.AddContractsFromNamespace(ns, internalVisibility))
                        {
                            config.Suffix = suffix;
                            config.ForceSync = forceSync;
                            config.ForceAsync = forceAsync;
                            config.AddExcluded(excludedContracts);
                        }
                    }
                    catch (Exception e)
                    {
                        return HandleError($"Failed to resolve contracts: {contract.White().Bold()}", e);
                    }
                }
                else
                {
                    try
                    {
                        var config = rootConfiguration.AddContract(contract, internalVisibility);
                        config.ForceSync = forceSync;
                        config.ForceAsync = forceAsync;
                        config.Suffix = suffix;
                        config.AddExcluded(excludedContracts);
                    }
                    catch (Exception e)
                    {
                        return HandleError($"Failed to resolve contract: {contract.White().Bold()}", e);
                    }
                }
            }

            return 0;
        }

        internal static int HandleError(string message, Exception e)
        {
            Console.WriteLine(Environment.NewLine);
            ErrorConsole.WriteLine(message.Red().Bold());
            Console.WriteLine(e.ToString());
            Console.WriteLine(Environment.NewLine);

            return 1;
        }

        private static RootConfiguration CreateSampleConfiguration(AssemblyCache cache)
        {
            RootConfiguration rootConfiguration = new RootConfiguration(cache);
            rootConfiguration.Modifier = "<public|internal>";
            rootConfiguration.Assemblies = new List<string> { "<AssemblyPath>", "<AssemblyPath>", "<AssemblyPath>" };
            rootConfiguration.Contracts = new List<InterfaceConfiguration>();
            rootConfiguration.Contracts.Add(
                new InterfaceConfiguration
                {
                    Contract = "<Type>",
                    Modifier = "<public|internal>",
                    Excluded = new List<string> {"<FullTypeName>", "<FullTypeName>"},
                    ForceAsync = true,
                    ForceSync = true,
                    Output = "<Path>",
                    Suffix = $"<Suffix> // suffix for generated interface, defaults to '{GeneratorBase.AsyncSuffix}'",
                    Namespace = "<Namespace> // namespace of generated interface, defaults to contract namespace if null",
                    Name = "<InterfaceName> // name of generated interface, defaults to '<ContractName><Suffix>' if null",
                    ExcludedInterfaces = new List<string> {"<FullTypeName>", "<FullTypeName>"}
                });

            return rootConfiguration;
        }

        private static string GetVersion()
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return assemblyInformationalVersionAttribute.InformationalVersion;
        }
    }
}
