using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Extensions.PlatformAbstractions;

namespace Bolt.Console
{
    public static class Program
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);
        private static readonly AnsiConsole ErrorConsole = AnsiConsole.GetError(true);

        private static readonly AssemblyCache Cache = new AssemblyCache(
            PlatformServices.Default.LibraryManager,
            PlatformServices.Default.AssemblyLoadContextAccessor,
            PlatformServices.Default.AssemblyLoaderContainer,
            PlatformServices.Default.Application);

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "bolt";
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
                    if (string.IsNullOrEmpty(input.Value) && !Cache.IsHosted())
                    {
                        Console.WriteLine("Assembly must be specified.".Yellow());
                        return 1;
                    }

                    if (!Cache.IsHosted() && !File.Exists(input.Value))
                    {
                        Console.WriteLine($"Assembly not found: {input.Value.White()}".Yellow());
                        return 1;
                    }

                    try
                    {
                        string json = RootConfig.CreateFromAssembly(Cache, input.Value, GenerateContractMode.All, false).Serialize();
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
                var values = Enum.GetValues(typeof(GenerateContractMode)).OfType<GenerateContractMode>().Select(v => v.ToString());
                var rawModeValues = string.Join(",", values);

                c.Description = "Generates code from assembly or from configuration file.";
                var input = c.Argument("[input]", "Path to the assembly or configuration file.");
                var output = c.Option("--output <DIRECTORY>", "Directory where the Bolt code will be generated. If directory is not specified then the input path directory will be used instead.", CommandOptionType.SingleValue);
                var dirOption = c.Option("--dir <PATH>", "Directories where contract assemblies are located.", CommandOptionType.MultipleValue);
                var contractOption = c.Option("--contract <NAME>", "Additional contracts to generate, if not included in config file or assembly.", CommandOptionType.MultipleValue);
                var modeOption = c.Option($"--mode <{rawModeValues}>", "Specifies what parts of contracts should be generated. ", CommandOptionType.SingleValue);
                var internalSwitch = c.Option("--internal", "Generates the contracts with internal visibility.", CommandOptionType.NoValue);

                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    bool inputMustExist = !Cache.IsHosted();
                    bool inputExists = !string.IsNullOrEmpty(input.Value);

                    if (inputMustExist && !inputExists)
                    {
                        Console.WriteLine("Input path must be specified.".Yellow());
                        return 1;
                    }

                    var extension = inputExists ? Path.GetExtension(input.Value) : null;

                    if (inputMustExist && !File.Exists(input.Value))
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

                    GenerateContractMode mode;
                    try
                    {
                        mode = modeOption.HasValue() ? (GenerateContractMode)Enum.Parse(typeof(GenerateContractMode), modeOption.Value(), true) : GenerateContractMode.All;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Invalid mode option specified: {modeOption.Value().White().Bold()}, Available options: {rawModeValues.Bold()}".Yellow());
                        return 1;
                    }

                    bool asInternal = internalSwitch.HasValue();

                    RootConfig rootConfig;
                    if ( inputExists)
                    {
                        if (extension == ".exe" || extension == ".dll")
                        {
                            rootConfig = new RootConfig(Cache);
                            try
                            {
                                Console.WriteLine($"Loading all contracts from assembly: ${input.Value}");
                                Cache.Loader.Load(input.Value);
                                rootConfig.Assemblies.Add(input.Value);
                            }
                            catch (Exception e)
                            {
                                return HandleError($"Failed to read assembly: {input.Value.White().Bold()}", e);
                            }

                            if (AddContracts(rootConfig, contractOption.Values, mode, asInternal) != 0)
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            try
                            {
                                rootConfig = RootConfig.CreateFromConfig(Cache, input.Value);
                            }
                            catch (Exception e)
                            {
                                return HandleError($"Failed to read Bolt configuration: {input.Value.White().Bold()}", e);
                            }
                        }
                    }
                    else
                    {
                        rootConfig = new RootConfig(Cache);
                        if (AddContracts(rootConfig, contractOption.Values, mode, asInternal) != 0)
                        {
                            return 1;
                        }
                    }

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


            try
            {
                var code = app.Execute(args);
                return code;
            }
            finally
            {
                Cache.Dispose();
            }
        }

        private static int AddContracts(RootConfig rootConfig, List<string> contracts, GenerateContractMode mode, bool internalVisibility)
        {
            if (!contracts.Any() || contracts.Any(c => c.EndsWith(".*", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    rootConfig.AddAllContracts(mode, internalVisibility);
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
                        rootConfig.AddContractsFromNamespace(ns, mode, internalVisibility);
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
                        rootConfig.AddContract(contract, mode, internalVisibility);
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

        private static RootConfig CreateSampleConfiguration(AssemblyCache cache)
        {
            RootConfig rootConfig = new RootConfig(cache);
            rootConfig.Modifier = "<public|internal>";
            rootConfig.Assemblies = new List<string> { "<AssemblyPath>", "<AssemblyPath>", "<AssemblyPath>" };
            rootConfig.Generators = new List<GeneratorConfig>
                                        {
                                            new GeneratorConfig
                                                {
                                                    Name = "<GeneratorName>",
                                                    Type = "<FullTypeName>",
                                                    Properties =
                                                        new Dictionary<string, string>
                                                            {
                                                                {
                                                                    "<Name>",
                                                                    "<Value>"
                                                                }
                                                            }
                                                }
                                        };

            rootConfig.Contracts = new List<ProxyConfig>();
            rootConfig.Contracts.Add(
                new ProxyConfig
                    {
                        Contract = "<Type>",
                        Modifier = "<public|internal>",
                        Context = "<Context> // passed to user code generators",
                        Excluded = new List<string> { "<FullTypeName>", "<FullTypeName>" },
                        ForceAsync = true,
                        Generator = "<GeneratorName>",
                        Output = "<Path>",
                        Suffix = "<Suffix> // suffix for generated client proxy, defaults to 'Proxy'",
                        Namespace = "<Namespace> // namespace of generated proxy, defaults to contract namespace if null",
                        Name = "<ProxyName> // name of generated proxy, defaults to 'ContractName + Suffix' if null",
                        ExcludedInterfaces = new List<string> { "<FullTypeName>", "<FullTypeName>" }
                    });

            return rootConfig;
        }

        private static string GetVersion()
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return assemblyInformationalVersionAttribute.InformationalVersion;
        }
    }
}
