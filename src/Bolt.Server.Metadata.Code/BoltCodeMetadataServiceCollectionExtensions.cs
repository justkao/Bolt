using Bolt.Server.Generator;
using Bolt.Server.Metadata;

namespace Microsoft.Framework.DependencyInjection
{
    public static class BoltCodeMetadataServiceCollectionExtensions
    {
        public static IServiceCollection AddCodeGenerator(this IServiceCollection collection)
        {
           return collection.AddTransient<IBoltMetadataHandler, BoltCodeMetadataHandler>();
        }
    }
}