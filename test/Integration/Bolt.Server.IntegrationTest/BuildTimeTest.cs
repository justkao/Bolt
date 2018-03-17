using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public static class BuildTimeTest
    {
        static BuildTimeTest()
        {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            var tmp = nameof(ITestContractInnerAsync.SimpleFunction2);
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            tmp = nameof(ITestContractInnerAsync.SimpleMethodWithComplexParameterAsync);
            tmp = nameof(ITestContractInnerAsync.MethodWithNotSerializableTypeAsync);
            tmp = nameof(ITestContractInnerAsync.FunctionWithNotSerializableTypeAsync);

            // check whether ITestContractAsync derives from ITestContractInnerAsync
            tmp = nameof(ITestContractAsync.SimpleFunction2);
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