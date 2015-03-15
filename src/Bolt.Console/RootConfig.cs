using Bolt.Generators;
using Microsoft.Framework.Runtime.Common.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() };

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
            RootConfig root = new RootConfig(cache);
            root.Assemblies = new List<string>() { Path.GetFullPath(assembly) };
            root.Contracts = new List<ContractConfig>();
            
            foreach (TypeInfo type in root.AssemblyCache.GetTypes(root.AssemblyCache.Add(assembly)))
            {
                if (!type.IsInterface)
                {
                    continue;
                }

                ContractConfig c = new ContractConfig();
                c.Parent = root;
                c.Contract = type.FullName;
                c.Client = new ClientConfig()
                {
                    ForceAsync = false,
                    Modifier = "public",
                    Suffix = "Proxy",
                    Namespace = type.Namespace
                };

                c.Descriptor = new DescriptorConfig()
                {
                    Modifier = "public",
                    Suffix = "Invoker",
                    Namespace = type.Namespace
                };

                c.Server = new ServerConfig()
                {
                    Modifier = "public",
                    Suffix = "Invoker",
                    Namespace = type.Namespace
                };

                root.Contracts.Add(c);
            }

            return root;
        }

        [JsonIgnore]
        public AssemblyCache AssemblyCache { get; set; }

        [JsonProperty(Required = Required.Always)]
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
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        public int Generate()
        {
            List<string> directories =
                Assemblies.Select(Path.GetDirectoryName)
                    .Concat(new[] { Directory.GetCurrentDirectory() })
                    .Distinct()
                    .ToList();

            foreach (string assembly in Assemblies)
            {
                AssemblyCache.Add(assembly);
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
            AnsiConsole.Output.WriteLine($"Generating files ... ");

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
                throw new ArgumentNullException("output");
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
        /*
        private class AssemblyResolver
        {
            private readonly string[] _directories;
            private readonly ConcurrentDictionary<string, List<string>> _directoryFiles = new ConcurrentDictionary<string, List<string>>();

            public AssemblyResolver(params string[] directories)
            {
                _directories = directories.Where(d => !string.IsNullOrEmpty(d)).Distinct().ToArray();
            }

            public Assembly Resolve(object sender, ResolveEventArgs e)
            {
                Assembly result = DoResolve(e);
                if (result == null)
                {
                    System.Console.WriteLine("Assembly not resolved: {0}", e.Name);
                }

                return result;
            }

            private Assembly DoResolve(ResolveEventArgs e)
            {
                if (e.RequestingAssembly != null)
                {
                    AssemblyName[] references = e.RequestingAssembly.GetReferencedAssemblies();

                    foreach (AssemblyName name in references)
                    {
                        foreach (string dir in _directories)
                        {
                            if (TryLoadAssembly(name.Name, GetFiles(dir)) != null)
                            {
                                break;
                            }
                        }
                    }
                }

                foreach (string dir in _directories.Where(p => !string.IsNullOrEmpty(p)))
                {
                    Assembly ass = TryLoadAssembly(e.Name, GetFiles(dir));
                    if (ass != null)
                    {
                        return ass;
                    }
                }

                return null;
            }

            private IEnumerable<string> GetFiles(string dir)
            {
                return _directoryFiles.GetOrAdd(
                    dir,
                    d =>
                    {
                        DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(dir));
                        return
                            directory.GetFiles("*.dll", SearchOption.AllDirectories)
                                .Concat(directory.GetFiles("*.exe", SearchOption.AllDirectories))
                                .Select(f => f.FullName)
                                .ToList();
                    });
            }

            private static Assembly TryLoadAssembly(string fullName, IEnumerable<string> files)
            {
                try
                {
                    Assembly assemblyFound = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == fullName);
                    if (assemblyFound != null)
                    {
                        return assemblyFound;
                    }

                    fullName = fullName.Split(new[] { ',' })[0];

                    string found = files.FirstOrDefault(f => string.Equals(fullName, Path.GetFileNameWithoutExtension(f), StringComparison.OrdinalIgnoreCase));

                    if (found == null)
                    {
                        return null;
                    }

                    return _AppDomain.Load(File.ReadAllBytes(found));
                }
                catch (Exception)
                {
                    Debug.Assert(false, "Assembly load failed.");
                    return null;
                }
            }

        }
        */

        public IUserCodeGenerator GetGenerator(string generatorName)
        {
            GeneratorConfig found =
                Generators.EmptyIfNull().FirstOrDefault(
                    g => string.Equals(g.Name, generatorName, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                throw new InvalidOperationException(string.Format("GeneratorEx '{0}' is not registered.", generatorName));
            }

            return found.GetGenerator();
        }
    }
}