using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Test.Common;

namespace Bolt.Server.IntegrationTest.Core
{
    public interface ITestContractInner : IExcludedContract
    {
        [AsyncOperation]
        void SimpleMethodWithComplexParameter(CompositeType compositeType);

        int SimpleFunction2();

        int SimpleFunctionWithCancellation(CancellationToken cancellation);

        [SyncOperation]
        List<CompositeType> FunctionReturningHugeData();

        void MethodTakingHugeData(List<CompositeType> arg);

        [AsyncOperation]
        void MethodWithNotSerializableType(NotSerializableType arg);

        [AsyncOperation]
        NotSerializableType FunctionWithNotSerializableType();

        [SyncOperation]
        Task<int> SimpleFunctionAsync();

        void MethodWithManyArguments(CompositeType arg1, CompositeType arg2, DateTime time);
    }
}
