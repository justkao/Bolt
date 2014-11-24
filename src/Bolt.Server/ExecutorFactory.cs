namespace Bolt.Server
{
    public class ExecutorFactory<TExecutor> where TExecutor : IContractInvoker, new()
    {
        public virtual IContractInvoker Create(ServerConfiguration configuration, IInstanceProvider instanceProvider)
        {
            ContractInvoker contractInvoker = (ContractInvoker)((object)new TExecutor());

            contractInvoker.Init();

            contractInvoker.DataHandler = configuration.ServerDataHandler;
            contractInvoker.ResponseHandler = configuration.ResponseHandler;
            contractInvoker.InstanceProvider = instanceProvider;

            return contractInvoker;
        }
    }
}
