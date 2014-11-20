
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Service.Test.Core
{
    public interface IExcludedContract
    {
        void ThisMethodShouldBeExcluded();
    }

    public interface ITestContractInner : IExcludedContract
    {
        [AsyncOperation]
        void SimpleMethodWithComplexParameter(CompositeType compositeType);

        int SimpleFunction();
    }

    public interface ITestContract : ITestContractInner
    {
        void SimpleMethodWithSimpleArguments(int val);

        [AsyncOperation]
        void SimpleMethod();

        Task SimpleMethodExAsync();

        void SimpleMethodWithCancellation(CancellationToken cancellation);

        CompositeType ComplexFunction();
    }
}
