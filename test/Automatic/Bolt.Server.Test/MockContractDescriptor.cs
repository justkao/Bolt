using System.Reflection;

namespace Bolt.Server.Test
{
    public class MockContractDescriptor : ContractDescriptor
    {
        public MockContractDescriptor() : base(typeof(IMockContract))
        {
            Init = Add("Init", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Init"));
            Action = Add("Action", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Action"));
            Destroy = Add("Destroy", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Destroy"));
        }

        public ActionDescriptor Init { get; }

        public ActionDescriptor Action { get; }

        public ActionDescriptor Destroy { get; }
    }
}
