using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Runtime.Common.CommandLine;
using System.Diagnostics;

namespace Bolt.Console
{
#if !NET45
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable, Microsoft.Framework.Runtime.IAssemblyLoader
    {
        private readonly Microsoft.Framework.Runtime.IAssemblyLoadContext _loadContext;
        private readonly Microsoft.Framework.Runtime.IAssemblyLoaderContainer _container;
        private readonly Microsoft.Framework.Runtime.ILibraryManager _libraryManager;
        private readonly Microsoft.Framework.Runtime.IApplicationEnvironment _environment;
        private readonly IDisposable _loaderRegistration = null;
#else
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable
    {
#endif
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _dirs = new HashSet<string>();

#if !NET45
        public AssemblyCache(Microsoft.Framework.Runtime.ILibraryManager manager, Microsoft.Framework.Runtime.IAssemblyLoadContextAccessor accessor, Microsoft.Framework.Runtime.IAssemblyLoaderContainer container, Microsoft.Framework.Runtime.IApplicationEnvironment environment)
        {
            _libraryManager = manager;
            _environment = environment;
            if (environment?.ApplicationName == "Bolt.Console")
            {
                _loadContext = accessor.GetLoadContext(typeof(Program).GetTypeInfo().Assembly);
                _container = container;
                _loaderRegistration = _container.AddLoader(this);
            }
        }
#else
        public AssemblyCache()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => Load(e.Name.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries).First().Trim());
        }
#endif

        public Assembly CurrentAssembly
        {
            get
            {
#if !NET45
                AnsiConsole.Output.WriteLine(_environment.ApplicationName);
#endifs
                // TODO:
                return null;
            }
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
                AnsiConsole.Output.WriteLine($"Directory '{dir.Bold()}' added to assembly search paths.");
                _dirs.Add(dir);
            }
        }

        public Assembly Load(string assembly)
        {
#if !NET45
            if (_loadContext == null)
            {
                var libraries = _libraryManager.GetLibraries().SelectMany(l => l.LoadableAssemblies).ToList();
                foreach (var item in libraries)
                {
                    AnsiConsole.Output.WriteLine(item.FullName);
                }
            }
#endif
            var originalName = assembly;
            if (!File.Exists(assembly))
            {
                assembly = FindAssembly(assembly);
                if (string.IsNullOrEmpty(assembly))
                {
                    AnsiConsole.Output.WriteLine($"Assembly {originalName} could not be located.".Yellow());
                    throw new InvalidOperationException($"Assembly {originalName} not found.");
                }
            }

            string assemblyName = Path.GetFileName(assembly);
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
            AnsiConsole.Output.WriteLine($"Assembly loaded: {assemblyName.Bold()}");
            return loadedAssembly;
        }

        public IEnumerable<TypeInfo> GetTypes(Assembly assembly)
        {
            return assembly.ExportedTypes.Select(t => t.GetTypeInfo());
        }

        public Type GetType(string fullName)
        {
            return GetType(fullName, true);
        }

        public Type GetType(string fullName, bool throwError)
        {
            Type type = null;
            foreach (Assembly assembly in _assemblies.Values)
            {
                type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            type = Type.GetType(fullName);
            if (type == null && throwError)
            {
                throw new InvalidOperationException($"Type '{fullName}' could not be loaded.");
            }

            return type;
        }

        public IEnumerator<Assembly> GetEnumerator()
        {
            return _assemblies.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string FindAssembly(string name)
        {
            name = name.Trim();
            string extension = Path.GetExtension(name);
            bool hasExtension = false;

            if (!string.IsNullOrEmpty(extension))
            {
                hasExtension = new[] { ".exe", ".dll" }.Any(ext => string.CompareOrdinal(extension.ToLowerInvariant(), ext) == 0);
            }

            foreach (var dir in _dirs)
            {
                AnsiConsole.Output.WriteLine($"Lookup of assembly '{name.Bold()}' in directory '{dir.Bold()}'.");

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