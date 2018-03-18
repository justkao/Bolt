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

        [InlineData("Async", "Async")]
        [InlineData("AbcAsync", "Abc")]
        [InlineData("AbcAsyncSomething", "AbcAsyncSomething")]
        [InlineData("AbcAsyncA", "AbcAsyncA")]
        [InlineData("AbcAsyn", "AbcAsyn")]
        [InlineData("AbcASYNC", "Abc")]
        [InlineData("AbcASYNCSomething", "AbcASYNCSomething")]
        [Theory]
        public void NormalizeActionName(string input, string expected)
        {
            Assert.Equal(expected, BoltFramework.NormalizeActionName(input.AsReadOnlySpan()).ConvertToString());
        }


        public interface IInvalidInterface
        {
            void Method(string arg);
            void Method();
        }
    }
}