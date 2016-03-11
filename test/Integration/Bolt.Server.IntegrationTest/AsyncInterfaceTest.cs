using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class AsyncInterfaceTest : IntegrationTestBase
    {
        public Mock<IDummy> Contract = new Mock<IDummy>();

        [Fact]
        public void CreateProxy_DoesNotThrow()
        {
            CreateProxy();
        }

        [Fact]
        public void ValidationCall_Ok()
        {
            var proxy = CreateProxy();
            Contract.Setup(c => c.DoSomething(99, It.IsAny<CancellationToken>())).Returns<int, CancellationToken>((v, c) => v).Verifiable();

            int result = proxy.DoSomething(99, CancellationToken.None);
            Assert.Equal(99, result);

            Contract.Verify();
        }

        [Fact]
        public async Task Call_Ok()
        {
            var proxy = CreateProxy();
            Contract.Setup(c => c.DoSomething(99, It.IsAny<CancellationToken>())).Returns<int, CancellationToken>((v, c) => v).Verifiable();

            int result = await proxy.DoSomethingAsync(99, CancellationToken.None);
            Assert.Equal(99, result);

            Contract.Verify();
        }

        [Fact]
        public async Task Call_ShouldFail()
        {
            var proxy = CreateProxy();
            var error = await Assert.ThrowsAsync<BoltServerException>(() => proxy.DoSomethingAsyncInvalid(99, CancellationToken.None));
            Assert.Equal(ServerErrorCode.ActionNotFound, error.Error);
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(
                b =>
                    {
                        // we are using IDummy inteface on server and IDummyAsync on client, calls should be properly mapped
                        MockInstanceProvider instanceProvider = new MockInstanceProvider { CurrentInstance = Contract.Object };
                        b.Use<IDummy>(instanceProvider);
                    });
        }

        private IDummyAsync CreateProxy()
        {
            return ClientConfiguration.ProxyBuilder().Url(ServerUrl).Build<IDummyAsync>();
        }

        public interface IDummy
        {
            int DoSomething(int arg, CancellationToken cancellation);
        }

        public interface IDummyAsync : IDummy
        {
            Task<int> DoSomethingAsync(int arg, CancellationToken cancellation);

            Task<int> DoSomethingAsyncInvalid(int arg, CancellationToken cancellation);
        }
    }
}
