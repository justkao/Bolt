using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Bolt.Console
{
    public class Program
    {
        private readonly AssemblyCache _cache;

#if !NET45
        public Program(IServiceProvider provider, Microsoft.Framework.Runtime.ILibraryManager manager, Microsoft.Framework.Runtime.IAssemblyLoadContextAccessor accessor, Microsoft.Framework.Runtime.IAssemblyLoaderContainer container, Microsoft.Framework.Runtime.IApplicationEnvironment environment)
        {
            _cache = new AssemblyCache(manager, accessor, container, environment);
        }
#else
        public Program()
        {
            _cache = new AssemblyCache();
        }
#endif
        public int Main(string[] args)
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
                        string result = CreateSampleConfiguration(_cache).Serialize();
                        string outputFile = PathHelpers.GetOutput(Directory.GetCurrentDirectory(), argRoot.Value, "bolt.example.json");
                        bool exist = File.Exists(outputFile);
                        File.WriteAllText(outputFile, result);
                        if (exist)
                        {
                            AnsiConsole.Output.WriteLine($"Examle overwritten: {outputFile.White().Bold()}".Green().Bold());
                        }
                        else
                        {
                            AnsiConsole.Output.WriteLine($"Example created: {outputFile.White().Bold()}".Green());
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
                    if (string.IsNullOrEmpty(input.Value) && !_cache.IsHosted())
                    {
                        AnsiConsole.Output.WriteLine("Assembly must be specified.".Yellow());
                        return 1;
                    }

                    if (!_cache.IsHosted() && !File.Exists(input.Value))
                    {
                        AnsiConsole.Output.WriteLine($"Assembly not found: {input.Value.White()}".Yellow());
                        return 1;
                    }

                    try
                    {
                        string json = RootConfig.CreateFromAssembly(_cache, input.Value, GenerateContractMode.All, false).Serialize();
                        var outputFile = PathHelpers.GetOutput(Path.GetDirectoryName(input.Value), output.Value(), "bolt.configuration.json");
                        bool exist = File.Exists(outputFile);
                        File.WriteAllText(outputFile, json);
                        if (exist)
                        {
                            AnsiConsole.Output.WriteLine($"Configuration overwritten: {outputFile.White().Bold()}".Green().Bold());
                        }
                        else
                        {
                            AnsiConsole.Output.WriteLine($"Configuration created: {outputFile.White().Bold()}".Green());
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
                    bool inputMustExist = !contractOption.Values.Any();
                    bool inputExists = !string.IsNullOrEmpty(input.Value);

                    if (inputMustExist && !inputExists)
                    {
                        AnsiConsole.Output.WriteLine("Input path must be specified.".Yellow());
                        return 1;
                    }

                    var extension = inputExists ? Path.GetExtension(input.Value) : null;

                    if (inputMustExist && !File.Exists(input.Value))
                    {
                        AnsiConsole.Output.WriteLine($"File not found: {input.Value.White().Bold()}".Yellow());
                        return 1;
                    }

                    if (inputExists)
                    {
                        _cache.AddDirectory(Path.GetDirectoryName(Path.GetFullPath(input.Value)));
                    }

                    foreach (var dir in dirOption.Values)
                    {
                        if (Directory.Exists(dir))
                        {
                            _cache.AddDirectory(dir);
                        }
                    }
                    var mode = GenerateContractMode.All;
                    try
                    {
                        mode = modeOption.HasValue() ? (GenerateContractMode)Enum.Parse(typeof(GenerateContractMode), modeOption.Value(), true) : GenerateContractMode.All;
                    }
                    catch (Exception)
                    {
                        
                        AnsiConsole.Output.WriteLine($"Invalid mode option specified: {modeOption.Value().White().Bold()}, Available options: {rawModeValues.Bold()}".Yellow());
                        return 1;
                    }

                    bool asInternal = internalSwitch.HasValue() ? true : false;

                    RootConfig rootConfig;

                    if (extension == ".exe" || extension == ".dll" || !inputExists)
                    {
                        try
                        {
                            rootConfig = RootConfig.CreateFromAssembly(_cache, inputExists ? input.Value : null, mode, asInternal);
                            rootConfig.IgnoreGeneratorErrors = true;
                        }
                        catch(Exception e)
                        {
                            return HandleError($"Failed to read assembly: {input.Value.White().Bold()}", e);
                        }
                    }
                    else
                    {
                        try
                        {
                            rootConfig = RootConfig.CreateFromConfig(_cache, input.Value);
                        }
                        catch (Exception e)
                        {
                            return HandleError($"Failed to read Bolt configuration: {input.Value.White().Bold()}", e);
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

                    foreach (var contract in contractOption.Values)
                    {
                        try
                        {
                            rootConfig.AddContract(contract, mode, asInternal);
                        }
                        catch (Exception e)
                        {
                            return HandleError($"Failed to resolve contract: {contract.White().Bold()}", e);
                        }
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
                _cache.Dispose();
            }
        }

        internal static int HandleError(string message, Exception e)
        {
            AnsiConsole.Output.WriteLine(Environment.NewLine);
            AnsiConsole.Error.WriteLine(message.Red().Bold());
            AnsiConsole.Output.WriteLine(e.ToString());
            AnsiConsole.Output.WriteLine(Environment.NewLine);

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
                                                    Properties = new Dictionary<string, string>
                                                    {
                                                                         { "<Name>", "<Value>" }
                                                                     }
                                                }
                                        };

            rootConfig.Contracts = new List<ContractConfig>();
            rootConfig.Contracts.Add(new ContractConfig
            {
                Contract = "<Type>",
                Modifier = "<public|internal>",
                Context = "<Context> // passed to user code generators",
                Excluded = new List<string> { "<FullTypeName>", "<FullTypeName>" },
                Client = new ClientConfig
                {
                    ForceAsync = true,
                    Generator = "<GeneratorName>",
                    Output = "<Path>",
                    Excluded = new List<string> { "<FullTypeName>", "<FullTypeName>" },
                    Suffix = "<Suffix> // suffix for generated client proxy, defaults to 'Proxy'",
                    Modifier = "<public|internal>",
                    Namespace = "<Namespace> // namespace of generated proxy, defaults to contract namespace if null",
                    Name = "<ProxyName> // name of generated proxy, defaults to 'ContractName + Suffix' if null",
                    ExcludedInterfaces = new List<string> { "<FullTypeName>", "<FullTypeName>" }
                },
                Server = new ServerConfig
                {
                    Output = "<Path>",
                    Excluded = new List<string> { "<FullTypeName>" },
                    Suffix = "<Suffix> // suffix for generated server invokers, defaults to 'Invoker'",
                    Modifier = "<public|internal>",
                    Namespace = "<Namespace> // namespace of generated server invoker, defaults to contract namespace if null",
                    Name = "<ProxyName> // name of generated server invoker, defaults to 'ContractName + Suffix' if null",
                    Generator = "<GeneratorName> // user defined generator for server invokers",
                    GeneratorEx = "<GeneratorName> // user defined generator for invoker extensions",
                    StateFullBase = "<FullTypeName> // base class used for statefull invokers",
                    GenerateExtensions = true
                },
                Descriptor = new DescriptorConfig
                {
                    Modifier = "<public|internal>",
                    Output = "<Path>"
                }
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
