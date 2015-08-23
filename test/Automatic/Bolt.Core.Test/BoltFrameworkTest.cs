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
        [InlineData("Async", "Async")]
        [InlineData("AbcASYNC", "Abc")]
        [InlineData("AbcASYNCSomething", "AbcASYNCSomething")]
        [InlineData("AbcAsyn", "AbcAsyn")]
        [Theory]
        public void TrimAsyncPostFix(string input, string expected)
        {
            string coerced;
            BoltFramework.TrimAsyncPostfix(input, out coerced);
            Assert.Equal(expected, coerced);
        }


        public interface IInvalidInterface
        {
            void Method(string arg);
            void Method();
        }
    }
}