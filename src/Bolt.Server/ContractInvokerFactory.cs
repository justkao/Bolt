namespace Bolt.Server
{
    public class ContractInvokerFactory<TInvoker> where TInvoker : IContractInvoker, new()
    {
        public virtual IContractInvoker Create(ServerConfiguration configuration, IInstanceProvider instanceProvider)
        {
            ContractInvoker contractInvoker = (ContractInvoker)((object)new TInvoker());

            contractInvoker.Init();

            contractInvoker.DataHandler = configuration.ServerDataHandler;
            contractInvoker.ResponseHandler = configuration.ResponseHandler;
            contractInvoker.InstanceProvider = instanceProvider;

            return contractInvoker;
        }
    }
}
