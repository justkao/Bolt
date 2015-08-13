using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Generators;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bolt.Console
{
    public class RootConfig
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);
        private readonly Dictionary<string, DocumentGenerator> _documents = new Dictionary<string, DocumentGenerator>();

        public RootConfig(AssemblyCache cache)
        {
            Contracts = new List<ProxyConfig>();
            AssemblyCache = cache;
            Generators = new List<GeneratorConfig>();
            Assemblies = new List<string>();
        }

        public static RootConfig CreateFromConfig(AssemblyCache cache, string file)
        {
            file = Path.GetFullPath(file);
            string content = File.ReadAllText(file);
            return CreateFromConfig(cache, Path.GetDirectoryName(file), content);
        }

        public static RootConfig CreateFromConfig(AssemblyCache cache, string outputDirectory, string content)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() };

            RootConfig config = JsonConvert.DeserializeObject<RootConfig>(content, settings);
            config.OutputDirectory = outputDirectory;
            config.AssemblyCache = cache;

            foreach (ProxyConfig contract in config.Contracts)
            {
                contract.Parent = config;
            }

            if (config.Generators != null)
            {
                foreach (GeneratorConfig generator in config.Generators)
                {
                    generator.Parent = config;
                }
            }

            return config;
        }

        public static RootConfig CreateFromAssembly(AssemblyCache cache, string assembly, GenerateContractMode mode, bool internalVisibility)
        {
            RootConfig root = new RootConfig(cache)
            {
                Contracts = new List<ProxyConfig>()
            };

            Assembly loadedAssembly = null;
            if (!string.IsNullOrEmpty(assembly) && File.Exists(assembly))
            {
                root.Assemblies = new List<string> { Path.GetFullPath(assembly) };
                loadedAssembly = cache.Loader.Load(assembly);
            }
            else
            {
                loadedAssembly = cache.HostedAssembly;
            }

            if (loadedAssembly != null)
            {
                foreach (var type in root.AssemblyCache.GetTypes(loadedAssembly))
                {
                    root.AddContract(type.GetTypeInfo(), mode, internalVisibility);
                };
            }

            return root;
        }

        [JsonIgnore]
        public AssemblyCache AssemblyCache { get; private set; }

        public List<string> Assemblies { get; set; }

        public List<GeneratorConfig> Generators { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<ProxyConfig> Contracts { get; set; }

        [JsonIgnore]
        public bool IgnoreGeneratorErrors { get; set; }

        public string Modifier { get; set; }

        public bool FullTypeNames { get; set; }

        [JsonIgnore]
        public string OutputDirectory { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(
                this,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        public void AddContractsFromNamespace(string ns, GenerateContractMode mode, bool internalVisibility)
        {
            foreach (var type in AssemblyCache.GetTypes(ns))
            {
                AddContract(type.GetTypeInfo(), mode, internalVisibility);
                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
        }

        public void AddAllContracts(GenerateContractMode mode, bool internalVisibility)
        {
            foreach (var type in AssemblyCache.GetTypes())
            {
                AddContract(type.GetTypeInfo(), mode, internalVisibility);
                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
        }

        public void AddContract(string name, GenerateContractMode mode, bool internalVisibility)
        {
            var type = AssemblyCache.GetType(name);
            var addedContract = AddContract(type.GetTypeInfo(), mode, internalVisibility);
            if (addedContract != null)
            {
                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
			else
			{
				Console.WriteLine($"Contract '{type.Name.Bold()}' not found.");
			}
        }

        public ProxyConfig AddContract(TypeInfo type, GenerateContractMode mode, bool internalVisibility)
        {
            if (type == null)
            {
                return null;
            }

            if (!type.IsInterface)
            {
                return null;
            }

            if (Contracts.FirstOrDefault(existing => existing.Contract == type.FullName) != null)
            {
                return null;
            }

            ProxyConfig c = new ProxyConfig
            {
                Parent = this,
                Contract = type.AssemblyQualifiedName,
                Modifier = internalVisibility ? "internal" : "public",
                ForceAsync = true,
                Namespace = type.Namespace,
                Mode = mode
            };

            Contracts.Add(c);
            return c;
        }

        public int Generate()
        {
            if (Assemblies != null)
            {
                List<string> directories =
                    Assemblies.Select(Path.GetDirectoryName)
                        .Concat(new[] { Directory.GetCurrentDirectory() })
                        .Distinct()
                        .ToList();

                foreach (string dir in directories)
                {
                    AssemblyCache.Loader.AddDirectory(dir);
                }

                foreach (string assembly in Assemblies)
                {
                    AssemblyCache.Loader.Load(assembly);
                }
            }

            Stopwatch watch = Stopwatch.StartNew();

            foreach (ProxyConfig contract in Contracts)
            {
                try
                {
                    contract.Generate();
                }
                catch (Exception e)
                {
                    if (!IgnoreGeneratorErrors)
                    {
                        return Program.HandleError($"Failed to generate contract: {contract.Contract.Bold().White()}", e);
                    }

                    Program.HandleError($"Skipped contract generation: {contract.Contract.Bold().White()}", e);
                }
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Generating files ... ");

            foreach (var filesInDirectory in _documents.GroupBy(f=>Path.GetDirectoryName(f.Key)))
            {
                Console.WriteLine(string.Join(string.Empty, Enumerable.Repeat("-", filesInDirectory.Key.Count() + 12).ToArray()));
                Console.WriteLine($"Directory: {filesInDirectory.Key.Bold().White()}");
                Console.WriteLine(Environment.NewLine);

                foreach (var documentGenerator in filesInDirectory)
                {
                    try
                    {
                        string status;

                        string result = documentGenerator.Value.GetResult();
                        if (File.Exists(documentGenerator.Key))
                        {
                            string prev = File.ReadAllText(documentGenerator.Key);
                            if (prev != result)
                            {
                                File.WriteAllText(documentGenerator.Key, result);
                                status = "Overwritten".Green().Bold();
                            }
                            else
                            {
                                status = "Skipped".White();
                            }
                        }
                        else
                        {
                            string directory = Path.GetDirectoryName(documentGenerator.Key);
                            if (directory != null && !Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            File.WriteAllText(documentGenerator.Key, result);
                            status = "Generated".Green();
                        }

                        Console.WriteLine($"{status}: {Path.GetFileName(documentGenerator.Key).White().Bold()}");
                    }
                    catch (Exception e)
                    {
                        return Program.HandleError($"File Generation Failed: {Path.GetFileName(documentGenerator.Key).White()}", e);
                    }
                }
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Status:");
            Console.WriteLine($"{(_documents.Count + " Files Generated,").Green().Bold()}  {watch.ElapsedMilliseconds}ms elapsed");
            Console.WriteLine(Environment.NewLine);

            return 0;
        }

        public DocumentGenerator GetDocument(string output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (!_documents.ContainsKey(output))
            {
                _documents[output] = new DocumentGenerator();
                if (FullTypeNames)
                {
                    _documents[output].Formatter.ForceFullTypeNames = true;
                }
            }

            return _documents[output];
        }

        public IUserCodeGenerator GetGenerator(string generatorName)
        {
            GeneratorConfig found =
                Generators.EmptyIfNull().FirstOrDefault(
                    g => string.Equals(g.Name, generatorName, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                throw new InvalidOperationException($"GeneratorEx '{generatorName}' is not registered.");
            }

            return found.GetGenerator();
        }
    }
}