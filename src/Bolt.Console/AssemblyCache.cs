using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Console
{
#if !NET45
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable, Microsoft.Framework.Runtime.IAssemblyLoader
    {
        private readonly Microsoft.Framework.Runtime.IAssemblyLoadContext _loadContext;
        private readonly Microsoft.Framework.Runtime.IAssemblyLoaderContainer _container;
        private readonly IDisposable _loaderRegistration = null;
#else
    public class AssemblyCache : IEnumerable<Assembly>, IDisposable
    {
#endif
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _dirs = new HashSet<string>();

        public AssemblyCache(IServiceProvider serviceProvider)
        {
#if !NET45
            _loadContext = ((Microsoft.Framework.Runtime.IAssemblyLoadContextAccessor)serviceProvider.GetService(typeof(Microsoft.Framework.Runtime.IAssemblyLoadContextAccessor))).GetLoadContext(typeof(Program).GetTypeInfo().Assembly);
            _container = ((Microsoft.Framework.Runtime.IAssemblyLoaderContainer)serviceProvider.GetService(typeof(Microsoft.Framework.Runtime.IAssemblyLoaderContainer)));
            _loaderRegistration = _container.AddLoader(this);
#else
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                return Load(e.Name.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries).First().Trim());
            };
#endif
        }

        public Assembly Load(string assemblyPath)
        {
            var original = assemblyPath;
            if (!File.Exists(assemblyPath))
            {
                assemblyPath = FindAssembly(assemblyPath);
            }

            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new FileNotFoundException($"Assembly {original} not found.");
            }

            assemblyPath = Path.GetFullPath(assemblyPath);
            string directory = Path.GetDirectoryName(assemblyPath);
            string assemblyName = Path.GetFileName(assemblyPath);

            Assembly assembly;
            if (_assemblies.TryGetValue(assemblyName, out assembly))
            {
                return assembly;
            }
#if !NET45         
            assembly = _loadContext.LoadFile(assemblyPath);
#else
            assembly = Assembly.LoadFrom(assemblyPath);
#endif 
            _assemblies[assemblyName] = assembly;
            _dirs.Add(directory);

            AnsiConsole.Output.WriteLine($"Assembly loaded: {assemblyName.Bold()}");
            return assembly;
        }

        public IEnumerable<TypeInfo> GetTypes(Assembly assembly)
        {
            return assembly.ExportedTypes.Select(t => t.GetTypeInfo());
        }

        public Type GetType(string fullName)
        {
            foreach (Assembly assembly in _assemblies.Values)
            {
                Type type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            throw new InvalidOperationException(string.Format("Type '{0}' could not be loaded.", fullName));
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
                hasExtension = new[] { ".exe", ".dll" }.Any(ext => string.CompareOrdinal(extension, ext) == 0);
            }

            foreach (var dir in _dirs)
            {
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