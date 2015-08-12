using System;
using System.Reflection;

namespace Bolt.Session
{
    public interface ISessionContractDescriptorProvider
    {
        SessionContractDescriptor Resolve(Type contract);

        MethodInfo InitSessionDummy { get; }

        MethodInfo DestroySessionDummy { get; }
    }

    public static class SessionContractDescriptorProviderExtensions
    {
        public static SessionContractDescriptor Resolve<T>(this ISessionContractDescriptorProvider provider) where T : class
        {
            return provider.Resolve(typeof(T));
        }
    }
}