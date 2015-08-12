using System;

using Xunit;

namespace Bolt.Core.Test
{
    public class BoltFrameworkTest
    {
        [Fact]
        public void ValidateInvalidContract_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => BoltFramework.ValidateContract(typeof(BoltFrameworkTest)));
        }

        [Fact]
        public void Validate_ContractWithSameActions_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => BoltFramework.ValidateContract(typeof(IInvalidInterface)));
        }

        public interface IInvalidInterface
        {
            void Method(string arg);
            void Method();
        }
    }
}