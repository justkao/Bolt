using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Common.CommandLine;

namespace Bolt.Console
{
    public class DirectoryLookupAssemblyLoader : IAssemblyLoader, IEnumerable<Assembly>
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _dirs = new HashSet<string>();
        private readonly IAssemblyLoadContext _loadContext;

        public DirectoryLookupAssemblyLoader(IAssemblyLoadContext loadContext)
        {
            _loadContext = loadContext;
        }

        public void AddDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            dir = Path.GetFullPath(dir);
            if (!_dirs.Contains(dir))
            {
                Console.WriteLine($"Directory '{dir.Bold()}' added to assembly search paths.");
                _dirs.Add(dir);
            }
        }

        public Assembly Load(AssemblyName assemblyName)
        {
            return Load(assemblyName.Name);
        }

        public Assembly Load(string assembly)
        {
            var originalName = assembly;
            if (!File.Exists(assembly))
            {
                assembly = FindAssembly(assembly);
                if (string.IsNullOrEmpty(assembly))
                {
                    Console.WriteLine($"Assembly {originalName} could not be located.".Yellow());
                    throw new InvalidOperationException($"Assembly {originalName} not found.");
                }
            }
            else
            {
                assembly = Path.GetFullPath(assembly);
            }

            string assemblyName = Path.GetFileName(assembly) ?? assembly;
            Assembly loadedAssembly;
            if (_loadedAssemblies.TryGetValue(assemblyName, out loadedAssembly))
            {
                return loadedAssembly;
            }

            loadedAssembly = _loadContext.LoadFile(assembly);
            _loadedAssemblies[assemblyName] = loadedAssembly;
            Console.WriteLine($"Assembly loaded: {assemblyName.Bold()}");
            return loadedAssembly;
        }

        public IEnumerator<Assembly> GetEnumerator()
        {
            return _loadedAssemblies.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _loadedAssemblies.Values.GetEnumerator();
        }

        private string FindAssembly(string name)
        {
            name = name.Trim();
            string extension = Path.GetExtension(name);
            bool hasExtension = false;

            if (!string.IsNullOrEmpty(extension))
            {
                hasExtension =
                    new[] { ".exe", ".dll" }.Any(ext => string.CompareOrdinal(extension.ToLowerInvariant(), ext) == 0);
            }

            foreach (var dir in _dirs)
            {
                Console.WriteLine($"Lookup of assembly '{name.Bold()}' in directory '{dir.Bold()}'.");

                string file = Path.Combine(dir, name);

                if (hasExtension)
                {
                    if (File.Exists(file))
                    {
                        return file;
                    }
                }
                else
                {
                    if (File.Exists(file + ".dll"))
                    {
                        return file + ".dll";
                    }

                    if (File.Exists(file + ".exe"))
                    {
                        return file + ".exe";
                    }
                }
            }

            return null;
        }
    }
}
