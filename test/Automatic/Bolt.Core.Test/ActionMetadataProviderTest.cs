using System;
using System.Reflection;
using Bolt.Metadata;
using Xunit;

namespace Bolt.Core.Test
{
    public class ActionMetadataProviderTest
    {
        [Fact]
        public void EnsureMethodTimeoutFromAttribute()
        {
            ActionMetadataProvider provider = new ActionMetadataProvider();

            var metadata = provider.Resolve(typeof(ITestContract).GetRuntimeMethod(nameof(ITestContract.Method1), Array.Empty<Type>()));
            Assert.Equal(TimeSpan.FromMilliseconds(9999), metadata.Timeout);
        }

        public interface ITestContract
        {
            [Timeout(9999)]
            void Method1();
        }

        public class TimeoutAttribute : Attribute
        {
            public TimeoutAttribute(int timeout)
            {
                Timeout = timeout;
            }

            public int Timeout { get; set; }
        }
    }
}
