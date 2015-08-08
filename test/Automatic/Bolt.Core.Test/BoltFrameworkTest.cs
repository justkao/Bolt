using Xunit;

namespace Bolt.Core.Test
{
    public class BoltFrameworkTest
    {
        [Fact]
        public void InitSession_Ok()
        {
            Assert.NotNull(BoltFramework.InitSessionAction);
        }

        [Fact]
        public void DestroySession_Ok()
        {
            Assert.NotNull(BoltFramework.DestroySessionAction);
        }
    }
}