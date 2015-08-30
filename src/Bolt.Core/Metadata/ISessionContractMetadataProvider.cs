using System;
using System.Reflection;

namespace Bolt.Metadata
{
    public interface ISessionContractMetadataProvider
    {
        SessionContractMetadata Resolve(Type contract);

        MethodInfo InitSessionDefault { get; }

        MethodInfo DestroySessionDefault { get; }
    }

    public static class SessionContractMetadataProviderExtensions
    {
        public static SessionContractMetadata Resolve<T>(this ISessionContractMetadataProvider provider) where T : class
        {
            return provider.Resolve(typeof(T));
        }
    }
}