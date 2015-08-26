using Xunit;

namespace Bolt.Core.Test
{
    public class ActionMetadataTest
    {
        [Fact]
        public void Resolve_EnsureSameInstance()
        {
            var instance1 = BoltFramework.ActionMetadata.Resolve(BoltFramework.SessionMetadata.InitSessionDummy);
            var instance2 = BoltFramework.ActionMetadata.Resolve(BoltFramework.SessionMetadata.InitSessionDummy);

            Assert.Same(instance2, instance1);
        }
    }
}
