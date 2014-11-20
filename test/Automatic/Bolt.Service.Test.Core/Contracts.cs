using System;

namespace Bolt.Service.Test.Core
{
    public static class Contracts
    {
        public static readonly ContractDefinition TestContract = new ContractDefinition(typeof(ITestContract), new Type[] { typeof(IExcludedContract) });
    }
}
