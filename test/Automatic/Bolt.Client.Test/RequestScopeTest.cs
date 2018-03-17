using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Bolt.Client.Test
{
    public class RequestScopeTest
    {
        [Fact]
        public void Create_TimeoutOk()
        {
            using (RequestScope scope = new RequestScope(TimeSpan.FromSeconds(1)))
            {
                Assert.Equal(TimeSpan.FromSeconds(1), scope.Timeout);

            }
        }

        [Fact]
        public void Create_CancellationOk()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            using (RequestScope scope = new RequestScope(cancellation: source.Token))
            {
                Assert.Equal(source.Token, scope.Cancellation);

            }
        }

        [Fact]
        public void Create_EnsureCurrent()
        {
            RequestScope scope = new RequestScope();

            try
            {
                Assert.Equal(RequestScope.Current, scope);
            }
            finally
            {
                scope.Dispose();
            }
        }

        [Fact]
        public void Create_EnsureCurrentInChildThread()
        {
            RequestScope scope = new RequestScope();

            try
            {
                Task.Run(
                    () =>
                        {
                            Assert.Equal(RequestScope.Current, scope);
                        }).GetAwaiter().GetResult();
            }
            finally
            {
                scope.Dispose();
            }
        }


        [Fact]
        public void Dispose_EnsureDisposed()
        {
            RequestScope scope = new RequestScope();
            scope.Dispose();
            Assert.Null(RequestScope.Current);
        }

        [Fact]
        public void Dispose_EnsureDisposedInChildThread()
        {
            RequestScope scope = new RequestScope();
            scope.Dispose();

            Task.Run(
                () =>
                    {
                        Assert.Null(RequestScope.Current);
                    }).GetAwaiter().GetResult();
        }
    }
}
