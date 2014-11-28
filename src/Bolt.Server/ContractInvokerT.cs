namespace Bolt.Server
{
    public class ContractInvoker<T> : ContractInvoker
        where T : ContractDescriptor
    {
        public ContractInvoker()
            : base(ContractDescriptor<T>.Instance)
        {
        }

        public new T Descriptor
        {
            get
            {
                return (T)base.Descriptor;
            }

            set
            {
                base.Descriptor = value;
            }
        }

    }
}