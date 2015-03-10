namespace Bolt.Server
{
    public abstract class ContractInvoker<T> : ContractInvoker
        where T : ContractDescriptor
    {
        protected ContractInvoker()
            : base(ContractDescriptor<T>.Instance)
        {
        }

        public new T Descriptor
        {
            get { return (T)base.Descriptor; }
            set { base.Descriptor = value; }
        }
    }
}