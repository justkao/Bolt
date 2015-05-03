using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Common;
using Bolt.Generators;
using Microsoft.Framework.Runtime.Common.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bolt.Console
{
    public class RootConfig
    {
        private readonly Dictionary<string, DocumentGenerator> _documents = new Dictionary<string, DocumentGenerator>();

        public RootConfig(AssemblyCache cache)
        {
            Contracts = new List<ContractConfig>();
            AssemblyCache = cache;
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

            foreach (ContractConfig contract in config.Contracts)
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

        public static RootConfig CreateFromAssembly(AssemblyCache cache, string assembly)
        {
            RootConfig root = new RootConfig(cache)
            {
                Assemblies = new List<string>(),
                Contracts = new List<ContractConfig>()
            };

            if (File.Exists(assembly))
            {
                root.Assemblies.Add(Path.GetFullPath(assembly));

                foreach (TypeInfo type in root.AssemblyCache.GetTypes(root.AssemblyCache.Load(assembly)))
                {
                    root.AddContract(type);
                }
            }

            return root;
        }

        [JsonIgnore]
        public AssemblyCache AssemblyCache { get; set; }

        public List<string> Assemblies { get; set; }

        public List<GeneratorConfig> Generators { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<ContractConfig> Contracts { get; set; }

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

        public void AddContract(string name)
        {
            var type = AssemblyCache.GetType(name);
            var addedContract = AddContract(type?.GetTypeInfo());
            if (addedContract != null)
            {
                AnsiConsole.Output.WriteLine($"Contract '{type.Name.Bold()}' added.");
            }
        }

        public ContractConfig AddContract(TypeInfo type)
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

            ContractConfig c = new ContractConfig
            {
                Parent = this,
                Contract = type.AssemblyQualifiedName,
                Client = new ClientConfig
                {
                    ForceAsync = true,
                    Namespace = type.Namespace
                },
                Descriptor = new DescriptorConfig
                {
                    Namespace = type.Namespace
                },
                Server = new ServerConfig
                {
                    Namespace = type.Namespace
                }
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
                    AssemblyCache.AddDirectory(dir);
                }

                foreach (string assembly in Assemblies)
                {
                    AssemblyCache.Load(assembly);
                }
            }

            Stopwatch watch = Stopwatch.StartNew();

            foreach (ContractConfig contract in Contracts)
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

            AnsiConsole.Output.WriteLine(Environment.NewLine);
            AnsiConsole.Output.WriteLine("Generating files ... ");

            foreach (var filesInDirectory in _documents.GroupBy(f=>Path.GetDirectoryName(f.Key)))
            {
                AnsiConsole.Output.WriteLine(string.Join(string.Empty, Enumerable.Repeat("-", filesInDirectory.Key.Count() + 12).ToArray()));
                AnsiConsole.Output.WriteLine($"Directory: {filesInDirectory.Key.Bold().White()}");
                AnsiConsole.Output.WriteLine(Environment.NewLine);

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

                        AnsiConsole.Output.WriteLine($"{status}: {Path.GetFileName(documentGenerator.Key).White().Bold()}");
                    }
                    catch (Exception e)
                    {
                        return Program.HandleError($"File Generation Failed: {Path.GetFileName(documentGenerator.Key).White()}", e);
                    }
                }
            }

            AnsiConsole.Output.WriteLine(Environment.NewLine);
            AnsiConsole.Output.WriteLine("Status:");
            AnsiConsole.Output.WriteLine($"{(_documents.Count + " Files Generated,").Green().Bold()}  {watch.ElapsedMilliseconds}ms elapsed");
            AnsiConsole.Output.WriteLine(Environment.NewLine);

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