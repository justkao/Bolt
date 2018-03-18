using Bolt.Metadata;
using System;
using System.Reflection;

namespace Bolt.Server.Test
{
    public static class ActionResolverExtensions
    {
        public static MethodInfo Resolve(this IActionResolver resolver, Type type, string name)
        {
            return resolver.Resolve(BoltFramework.GetContract(type), name.AsReadOnlySpan())?.Action;
        }
    }
}
