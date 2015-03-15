using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Console
{
    public class AssemblyCache : IEnumerable<Assembly>
    {
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

#if !NET45
        [Microsoft.Framework.Runtime.AssemblyNeutral]
        public interface IAssemblyLoadContext : IDisposable
        {
            Assembly Load(string name);
            Assembly LoadFile(string path);
            Assembly LoadStream(Stream assemblyStream, Stream assemblySymbols);
        }

        private readonly Microsoft.Framework.Runtime.IAssemblyLoadContext _loader;

        public AssemblyCache(IServiceProvider serviceProvider)
        {
            _loader = ((Microsoft.Framework.Runtime.IAssemblyLoadContextFactory)serviceProvider.GetService(typeof(Microsoft.Framework.Runtime.IAssemblyLoadContextFactory))).Create();
        }
#else
        public AssemblyCache(IServiceProvider serviceProvider)
        {
        }
#endif 
        public Assembly Add(string assemblyPath)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);
            Assembly assembly;
            if (_assemblies.TryGetValue(assemblyPath, out assembly))
            {
                return assembly;
            }
#if !NET45         
            assembly = _loader.LoadFile(assemblyPath);
#else
            assembly = Assembly.LoadFrom(assemblyPath);
#endif 
            _assemblies[assemblyPath] = assembly;

            AnsiConsole.Output.WriteLine($"Assembly loaded: {assemblyPath.Bold()}");
            return assembly;
        }

        public IEnumerable<TypeInfo> GetTypes(Assembly assembly)
        {
            List<string> directories = _assemblies.Keys.Select(Path.GetDirectoryName).Where(d => !string.IsNullOrEmpty(d)).ToList();

            while (true)
            {
                try
                {
                    return assembly.ExportedTypes.Select(t => t.GetTypeInfo());
                }
                catch (FileLoadException e)
                {
                    if (!TryLoadAssembly(e.FileName, directories))
                    {
                        throw e;
                    }
                }
                catch (FileNotFoundException e)
                {
                    if (!TryLoadAssembly(e.FileName, directories))
                    {
                        throw e;
                    }
                }
            }
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

        private bool TryLoadAssembly(string rawName, List<string> directories)
        {
            string fileName = rawName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).First().Trim();

            foreach(var dir in directories.Distinct())
            {
                var files = Directory.GetFiles(dir);
                var found = Directory.GetFiles(dir).Where(f => string.CompareOrdinal(Path.GetFileName(f), fileName + ".dll") == 0).FirstOrDefault();
                if (found == null)
                {
                    found = Directory.GetFiles(dir).Where(f => string.CompareOrdinal(Path.GetFileName(f), fileName + ".exe") == 0).FirstOrDefault();
                    if (found == null)
                    {
                        continue;
                    }
                }

                if (_assemblies.ContainsKey(found))
                {
                    return false;
                }

                try
                {
                    GetTypes(Add(found));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}