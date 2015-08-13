using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Test.Common;

namespace Bolt.Server.IntegrationTest.Core
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

        List<CompositeType> FunctionReturningHugeData();

        void MethodTakingHugeData(List<CompositeType> arg);

        [AsyncOperation]
        void MethodWithNotSerializableType(NotSerializableType arg);

        [AsyncOperation]
        NotSerializableType FunctionWithNotSerializableType();

        Task<int> SimpleAsyncFunction();

        void MethodWithManyArguments(CompositeType arg1, CompositeType arg2, DateTime time);
    }

    public interface ITestContract : ITestContractInner
    {
        void SimpleMethodWithSimpleArguments(int val);

        [AsyncOperation]
        void SimpleMethod();

        void MethodWithNullableArguments(string arg);

        Task SimpleMethodExAsync();

        void SimpleMethodWithCancellation(CancellationToken cancellation);

        CompositeType ComplexFunction();
    }
}
