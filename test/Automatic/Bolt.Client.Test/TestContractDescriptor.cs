namespace Bolt.Client.Test
{
    using System.Reflection;

    public class TestContractDescriptor : ContractDescriptor
    {
        public TestContractDescriptor():base(typeof(ITestContract), "TestContract")
        {
            this.Execute = this.Add("Execute", typeof(string), typeof(ITestContract).GetTypeInfo().GetMethod("Execute"));
        }

        public ActionDescriptor Execute { get; set; }
    }
}