using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Bolt.Console
{
    public class AssemblyCache : IEnumerable<Assembly>
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly List<string> _assemblyPath = new List<string>();

        public void Add(string assemblyPath)
        {
            if (_assemblyPath.Contains(assemblyPath))
            {
                return;
            }

            _assemblies.Add(Assembly.Load(File.ReadAllBytes(assemblyPath)));
            _assemblyPath.Add(assemblyPath);
        }


        public Type GetType(string fullName)
        {
            return TypeHelper.GetTypeOrThrow(fullName);
        }

        public IEnumerator<Assembly> GetEnumerator()
        {
            return _assemblies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}