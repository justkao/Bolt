using System.Reflection;

namespace Bolt.Server.Test
{
    public class MockContractDescriptor
    {
        public MockContractDescriptor() 
        {
            Action = typeof (IMockContract).GetTypeInfo().GetDeclaredMethod(nameof(IMockContract.Action));
        }

        public MethodInfo Action { get; }
    }
}
