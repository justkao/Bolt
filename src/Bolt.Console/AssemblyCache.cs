using Microsoft.Framework.Runtime;
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
        private readonly IAssemblyLoadContext _loader;

        public AssemblyCache(IAssemblyLoadContext loader)
        {
            _loader = loader;
        }

        public Assembly Add(string assemblyPath)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);

            Assembly assembly;
            if (_assemblies.TryGetValue(assemblyPath, out assembly))
            {
                return assembly;
            }

            assembly = _loader.LoadFile(assemblyPath);

            _assemblies.Add(assemblyPath, assembly);
            return assembly;
        }

        public IEnumerable<TypeInfo> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                var errors = e.LoaderExceptions.OfType<FileLoadException>().ToArray();
                throw e;
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
    }
}