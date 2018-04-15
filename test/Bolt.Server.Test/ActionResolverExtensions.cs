using System;
using System.Reflection;
using Bolt.Metadata;

namespace Bolt.Server.Test
{
    public static class ActionResolverExtensions
    {
        public static MethodInfo Resolve(this IActionResolver resolver, Type type, string name)
        {
            return resolver.Resolve(BoltFramework.GetContract(type), name.AsSpan())?.Action;
        }
    }
}
