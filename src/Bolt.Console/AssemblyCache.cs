using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if !NET45
using Microsoft.Framework.Runtime;
#endif
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Bolt.Console
{
#if !NET45
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable, IAssemblyLoader
    {
        private readonly IAssemblyLoadContext _loadContext;
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationEnvironment _environment;
        private readonly IDisposable _loaderRegistration;
#else
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable
    {
#endif
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _dirs = new HashSet<string>();

#if !NET45
        public AssemblyCache(ILibraryManager manager,
            IAssemblyLoadContextAccessor accessor,
            IAssemblyLoaderContainer container,
            IApplicationEnvironment environment)
        {
            _libraryManager = manager;
            _environment = environment;
            _loadContext = accessor.GetLoadContext(typeof (Program).GetTypeInfo().Assembly);
            _loaderRegistration = container.AddLoader(this);
        }

#else
        public AssemblyCache()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (s, e) => Load(e.Name.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).First().Trim());
        }
#endif
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);

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
            if (_assemblies.TryGetValue(assemblyName, out loadedAssembly))
            {
                return loadedAssembly;
            }
#if !NET45
            loadedAssembly = _loadContext.LoadFile(assembly);
#else
            loadedAssembly = Assembly.LoadFrom(assembly);
#endif
            _assemblies[assemblyName] = loadedAssembly;
            Console.WriteLine($"Assembly loaded: {assemblyName.Bold()}");
            return loadedAssembly;
        }

        public IEnumerable<TypeInfo> GetTypes(Assembly assembly)
        {
            return assembly.ExportedTypes.Select(t => t.GetTypeInfo());
        }

        public Type GetType(string fullName)
        {
            Type type;
            foreach (Assembly assembly in _assemblies.Values)
            {
                type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            type = Type.GetType(fullName) ?? ResolveTypeEx(fullName);

            if (type == null)
            {
                throw new InvalidOperationException($"Type '{fullName}' could not be loaded.");
            }

            return type;
        }

        private Type ResolveTypeEx(string fullName)
        {
#if NET45
            return null;
#else
            if (!IsHosted())
            {
                return null;
            }

            fullName = fullName.Trim();
            try
            {
                var assembly = HostedAssembly;
                if (assembly != null)
                {
                    var type = FindType(assembly, fullName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            catch (Exception)
            {
                // OK, continue with full scan
            }

            var assemblies = _libraryManager.GetLibraries().SelectMany(l => l.LoadableAssemblies).ToList();
            return
                assemblies.Select(item => _loadContext.Load(item.FullName))
                    .Select(assembly => FindType(assembly, fullName))
                    .FirstOrDefault(type => type != null);
#endif
        }

        public Assembly HostedAssembly
        {
            get
            {
#if !NET45
                try
                {
                    return _loadContext.Load(_environment.ApplicationName);
                }
                catch (Exception)
                {
                    return null;
                }
#else
                return null;
#endif
            }
        }

        private static Type FindType(Assembly assembly, string fullName)
        {
            var found = assembly.ExportedTypes.FirstOrDefault(t => t.FullName == fullName);
            return found ?? assembly.ExportedTypes.FirstOrDefault(t => t.Name == fullName);
        }

        public IEnumerator<Assembly> GetEnumerator()
        {
            return _assemblies.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsHosted()
        {
#if !NET45
            return _environment?.ApplicationName != "Bolt.Console";
#else
            return false;
#endif
        }

        private string FindAssembly(string name)
        {
            name = name.Trim();
            string extension = Path.GetExtension(name);
            bool hasExtension = false;

            if (!string.IsNullOrEmpty(extension))
            {
                hasExtension =
                    new[] {".exe", ".dll"}.Any(ext => string.CompareOrdinal(extension.ToLowerInvariant(), ext) == 0);
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

        public void Dispose()
        {
#if !NET45
            _loaderRegistration?.Dispose();
#endif
        }
    }
}