using System.Reflection;

namespace Bolt.Server.Test
{
    public class MockContractDescriptor
    {
        public MockContractDescriptor() 
        {
            Init = typeof (IMockContract).GetTypeInfo().GetDeclaredMethod(nameof(IMockContract.Init));
            Action = typeof (IMockContract).GetTypeInfo().GetDeclaredMethod(nameof(IMockContract.Action));
            Destroy = typeof (IMockContract).GetTypeInfo().GetDeclaredMethod(nameof(IMockContract.Destroy));
        }

        public MethodInfo Init { get; }

        public MethodInfo Action { get; }

        public MethodInfo Destroy { get; }
    }
}
