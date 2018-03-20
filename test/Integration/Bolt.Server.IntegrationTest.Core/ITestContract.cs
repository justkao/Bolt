using System.Threading;
using System.Threading.Tasks;
using Bolt.Test.Common;

namespace Bolt.Server.IntegrationTest.Core
{
    public interface ITestContract : ITestContractInner
    {
        void SimpleMethodWithSimpleArguments(int val);

        [AsyncOperation]
        void SimpleMethod();

        void MethodWithNullableArguments(string arg);

        [SyncOperation]
        Task SimpleMethodExAsync();

        void SimpleMethodWithCancellation(CancellationToken cancellation);

        void SimpleWithCancellationAsFirstArgument(CancellationToken cancellation, int value);

        [AsyncOperation]
        CompositeType ComplexFunction();
    }
}
