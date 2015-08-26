using System.Reflection;

namespace Bolt.Metadata
{
    public interface IActionMetadataProvider
    {
        ActionMetadata Resolve(MethodInfo action);
    }
}
