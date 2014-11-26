namespace Bolt.Server
{
    public class ContractInvokerFactory<TInvoker, TDescriptor>
        where TInvoker : IContractInvoker<TDescriptor>, new()
        where TDescriptor : ContractDescriptor
    {
        public virtual IContractInvoker<TDescriptor> Create(ServerConfiguration configuration, IInstanceProvider instanceProvider)
        {
            ContractInvoker<TDescriptor> contractInvoker = (ContractInvoker<TDescriptor>)((object)new TInvoker());

            contractInvoker.Init();

            contractInvoker.DataHandler = configuration.ServerDataHandler;
            contractInvoker.ResponseHandler = configuration.ResponseHandler;
            contractInvoker.InstanceProvider = instanceProvider;
            contractInvoker.ErrorCodesHeader = configuration.ServerErrorCodesHeader;

            return contractInvoker;
        }
    }
}
