using System;
using System.Reflection;

namespace Bolt.Metadata
{
    public interface ISessionContractMetadataProvider
    {
        MethodInfo InitSessionDefault { get; }

        MethodInfo DestroySessionDefault { get; }

        SessionContractMetadata Resolve(Type contract);
    }

    public static class SessionContractMetadataProviderExtensions
    {
        public static SessionContractMetadata Resolve<T>(this ISessionContractMetadataProvider provider) where T : class
        {
            return provider.Resolve(typeof(T));
        }
    }
}