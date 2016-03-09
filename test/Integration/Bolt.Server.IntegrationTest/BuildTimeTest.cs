using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public static class BuildTimeTest
    {
        static BuildTimeTest()
        {
            var tmp = nameof(ITestContractInnerAsync.Simple);
            tmp = nameof(ITestContractInnerAsync.SimpleMethodWithComplexParameterAsync);
            tmp = nameof(ITestContractInnerAsync.MethodWithNotSerializableTypeAsync);
            tmp = nameof(ITestContractInnerAsync.FunctionWithNotSerializableTypeAsync);

            // check whether ITestContractAsync derives from ITestContractInnerAsync
            tmp = nameof(ITestContractAsync.Simple);
            tmp = nameof(ITestContractAsync.SimpleMethodWithComplexParameterAsync);
            tmp = nameof(ITestContractAsync.MethodWithNotSerializableTypeAsync);
            tmp = nameof(ITestContractAsync.FunctionWithNotSerializableTypeAsync);

            // check whether ITestContractAsync contains generated methods
            tmp = nameof(ITestContractAsync.SimpleMethodEx);
            tmp = nameof(ITestContractAsync.ComplexFunctionAsync);
            tmp = nameof(ITestContractAsync.SimpleMethodAsync);
        }
    }
}