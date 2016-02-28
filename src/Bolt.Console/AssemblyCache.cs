using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Bolt.Console
{
    public class AssemblyCache : IDisposable
    {
        private readonly IAssemblyLoadContext _loadContext;
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationEnvironment _environment;
        private readonly IDisposable _loaderRegistration;

        public AssemblyCache(ILibraryManager manager,
            IAssemblyLoadContextAccessor accessor,
            IAssemblyLoaderContainer container,
            IApplicationEnvironment environment)
        {
            _libraryManager = manager;
            _environment = environment;
            _loadContext = accessor.GetLoadContext(typeof (Program).GetTypeInfo().Assembly);
            Loader = new DirectoryLookupAssemblyLoader(_loadContext);
            _loaderRegistration = container.AddLoader(Loader);
        }

        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);

        public DirectoryLookupAssemblyLoader Loader { get; }

        public Assembly HostedAssembly
        {
            get
            {
                try
                {
                    return _loadContext.Load(_environment.ApplicationName);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

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
            var found = assembly.DefinedTypes.Where(info =>
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

        private static Type FindType(Assembly assembly, string fullName)
        {
            var found = assembly.ExportedTypes.FirstOrDefault(t => t.FullName == fullName);
            return found ?? assembly.ExportedTypes.FirstOrDefault(t => t.Name == fullName);
        }

        public bool IsHosted()
        {
#if !NET45
            return _environment?.ApplicationName != "Bolt.Console";
#else
            return false;
#endif
        }

        private IEnumerable<Assembly> GetSearchableAssemblies()
        {
            // first return explicitely loaded assemblies
            foreach (var assembly in Loader)
            {
                yield return assembly;
            }

            // now hosted assembly
            if (HostedAssembly != null)
            {
                yield return HostedAssembly;
            }

            // then the rest
            foreach (var name in _libraryManager.GetLibraries().SelectMany(l => l.Assemblies))
            {
                if (Loader.FirstOrDefault(a=>a.GetName() == name) != null)
                {
                    continue;
                }

                yield return _loadContext.Load(name.FullName);
            }
        }


        public void Dispose()
        {
#if !NET45
            _loaderRegistration?.Dispose();
#endif
        }
    }
}