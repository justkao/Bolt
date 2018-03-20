using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.Tools
{
    public class AssemblyCache
    {
        public AssemblyCache()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            Loader = new DirectoryLookupAssemblyLoader();
        }

        public DirectoryLookupAssemblyLoader Loader { get; }

        public IEnumerable<Type> GetTypes(Assembly assembly)
        {
            return CoerceTypes(GetTypesInternal(assembly, null).ToList());
        }

        public IEnumerable<Type> GetTypes()
        {
            return CoerceTypes(GetTypesInternal(null).ToList());
        }

        public IEnumerable<Type> GetTypes(string ns)
        {
            return CoerceTypes(GetTypesInternal(ns).ToList());
        }

        public Type GetType(string fullName)
        {
            Type type;
            foreach (Assembly assembly in Loader)
            {
                type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            type = Type.GetType(fullName) ?? LoadAndResolveType(fullName);

            if (type == null)
            {
                throw new InvalidOperationException($"Type '{fullName}' could not be loaded.");
            }

            return type;
        }

        private static Type FindType(Assembly assembly, string fullName)
        {
            var found = assembly.ExportedTypes.FirstOrDefault(t => t.FullName == fullName);
            return found ?? assembly.ExportedTypes.FirstOrDefault(t => t.Name == fullName);
        }

        private void OnLoadAssembly(object sender, AssemblyLoadEventArgs args)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Type> CoerceTypes(IReadOnlyCollection<Type> initial)
        {
            var types = initial;

            foreach (var type in types)
            {
                // do not generate inner interfaces
                if (types.Any(t => t.GetTypeInfo().ImplementedInterfaces.Contains(type)))
                {
                    continue;
                }

                yield return type;
            }
        }

        private IEnumerable<Type> GetTypesInternal(string ns)
        {
            foreach (var assembly in GetSearchableAssemblies())
            {
                var types = GetTypesInternal(assembly, ns);
                if (types.Any())
                {
                    return types;
                }
            }

            return Enumerable.Empty<Type>();
        }

        private IEnumerable<Type> GetTypesInternal(Assembly assembly, string ns)
        {
            IEnumerable<TypeInfo> found = assembly.DefinedTypes.Where(info =>
            {
                if (!info.IsInterface)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(ns))
                {
                    return true;
                }

                return info.Namespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase);
            });

            if (found.Any())
            {
                return found.Select(t => t.AsType());
            }

            return Enumerable.Empty<Type>();
        }

        private Type LoadAndResolveType(string fullName)
        {
            fullName = fullName.Trim();

            foreach (var assembly in GetSearchableAssemblies())
            {
                try
                {
                    var found = FindType(assembly, fullName);
                    if (found != null)
                    {
                        return found;
                    }
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        private IEnumerable<Assembly> GetSearchableAssemblies()
        {
            // first return explicitely loaded assemblies
            return Loader;
        }

        private Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            return Loader.Load(new AssemblyName(args.Name).Name);
        }
    }
}