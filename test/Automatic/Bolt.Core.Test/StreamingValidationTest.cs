using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Pipeline;
using Xunit;

namespace Bolt.Core.Test
{
    public class StreamingValidationTest
    {
        [Fact]
        public void Validate_Ok()
        {
            new TestStreamingMiddleware().Validate(typeof(IValid));
        }

        [Fact]
        public void StringContentParameterType_ShouldThrow()
        {
            TestStreamingMiddleware middleware = new TestStreamingMiddleware();

            Assert.Throws<ContractViolationException>(() => middleware.Validate(typeof (IInvalid1)));
        }

        [Fact]
        public void MethodWithCustomArgument_ShouldThrow()
        {
            TestStreamingMiddleware middleware = new TestStreamingMiddleware();

            Assert.Throws<ContractViolationException>(() => middleware.Validate(typeof(IInvalid2)));
        }

        [Fact]
        public void StringContentResultType_ShouldThrow()
        {
            TestStreamingMiddleware middleware = new TestStreamingMiddleware();

            Assert.Throws<ContractViolationException>(() => middleware.Validate(typeof(IInvalid3)));
        }

        public class TestStreamingMiddleware : StreamingMiddlewareBase<ActionContextBase>
        {
            public override Task Invoke(ActionContextBase context)
            {
                throw new NotSupportedException();
            }
        }

        public interface IInvalid1
        {
            void GetContent(StringContent content);
        }

        public interface IInvalid2
        {
            void GetContent(HttpContent content, int someArgument);
        }

        public interface IInvalid3
        {
            StringContent GetContent();
        }

        public interface IValid
        {
            HttpContent GetContent();

            HttpContent GetContent2(CancellationToken cancellation);

            void PostContent(HttpContent content);

            void PostContent2(HttpContent content, CancellationToken cancellation);

            Task<HttpContent> GetContentAsync();

            Task<HttpContent> GetContent2Async(CancellationToken cancellation);

            Task PostContentAsync(HttpContent content);

            Task PostContent2Async(HttpContent content, CancellationToken cancellation);
        }
    }
}
