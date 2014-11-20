using System;

namespace Bolt.Service.Test.Core
{
    public partial class TestContractChannel : ITestContractAsync
    {
        public void ThisMethodShouldBeExcluded()
        {
            throw new NotImplementedException();
        }
    }
}