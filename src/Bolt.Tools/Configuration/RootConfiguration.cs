using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Tools.Generators;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bolt.Tools.Configuration
{
    public class RootConfiguration
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);
        private readonly Dictionary<string, DocumentGenerator> _documents = new Dictionary<string, DocumentGenerator>();

        public RootConfiguration(AssemblyCache cache)
        {
            Contracts = new List<InterfaceConfiguration>();
            AssemblyCache = cache;
            Assemblies = new List<string>();
        }

        [JsonIgnore]
        public AssemblyCache AssemblyCache { get; private set; }

        public List<string> Assemblies { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<InterfaceConfiguration> Contracts { get; set; }

        [JsonIgnore]
        public bool IgnoreGeneratorErrors { get; set; }

        public string Modifier { get; set; }

        public bool FullTypeNames { get; set; }

        [JsonIgnore]
        public string OutputDirectory { get; set; }

        public static RootConfiguration CreateFromConfig(AssemblyCache cache, string file)
        {
            file = Path.GetFullPath(file);
            string content = File.ReadAllText(file);
            return CreateFromConfig(cache, Path.GetDirectoryName(file), content);
        }

        public static RootConfiguration CreateFromConfig(AssemblyCache cache, string outputDirectory, string content)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() };

            RootConfiguration configuration = JsonConvert.DeserializeObject<RootConfiguration>(content, settings);
            configuration.OutputDirectory = outputDirectory;
            configuration.AssemblyCache = cache;

            foreach (InterfaceConfiguration contract in configuration.Contracts)
            {
                contract.Parent = configuration;
            }

            return configuration;
        }

        public static RootConfiguration CreateFromAssembly(AssemblyCache cache, string assembly, bool internalVisibility)
        {
            RootConfiguration root = new RootConfiguration(cache)
            {
                Contracts = new List<InterfaceConfiguration>()
            };

            Assembly loadedAssembly = null;
            if (!string.IsNullOrEmpty(assembly) && File.Exists(assembly))
            {
                root.Assemblies = new List<string> { Path.GetFullPath(assembly) };
                loadedAssembly = cache.Loader.Load(assembly);
            }

            if (loadedAssembly != null)
            {
                foreach (var type in root.AssemblyCache.GetTypes(loadedAssembly))
                {
                    root.AddContract(type.GetTypeInfo(), internalVisibility);
                }
            }

            return root;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(
                this,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        public IEnumerable<InterfaceConfiguration> AddContractsFromNamespace(string namespaceName, bool internalVisibility)
        {
            foreach (var type in AssemblyCache.GetTypes(namespaceName))
            {
                InterfaceConfiguration contract = AddContract(type.GetTypeInfo(), internalVisibility);
                if (contract != null)
                {
                    yield return contract;
                }

                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
        }

        public IEnumerable<InterfaceConfiguration> AddAllContracts(bool internalVisibility)
        {
            foreach (var type in AssemblyCache.GetTypes())
            {
                InterfaceConfiguration contract = AddContract(type.GetTypeInfo(), internalVisibility);
                if (contract != null)
                {
                    yield return contract;
                }

                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
        }

        public InterfaceConfiguration AddContract(string name, bool internalVisibility)
        {
            var type = AssemblyCache.GetType(name);
            var addedContract = AddContract(type.GetTypeInfo(), internalVisibility);
            if (addedContract != null)
            {
                Console.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
            else
            {
                Console.WriteLine($"Contract '{type.Name.Bold()}' not found.");
            }

            return addedContract;
        }

        public InterfaceConfiguration AddContract(TypeInfo type, bool internalVisibility)
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

            InterfaceConfiguration c = new InterfaceConfiguration
            {
                Parent = this,
                Contract = type.FullName,
                Modifier = internalVisibility ? "internal" : "public",
                ForceAsyncMethod = true,
                ForceSyncMethod = true,
                Namespace = type.Namespace
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

            foreach (InterfaceConfiguration contract in Contracts)
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

            foreach (var filesInDirectory in _documents.GroupBy(f => Path.GetDirectoryName(f.Key)))
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
    }
}