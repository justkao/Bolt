namespace Bolt.Server
{
    public class ExecutorFactory<TExecutor> where TExecutor : IExecutor, new()
    {
        public virtual IExecutor Create(ServerConfiguration configuration, IInstanceProvider instanceProvider)
        {
            Executor executor = (Executor)((object)new TExecutor());

            executor.Init();

            executor.DataHandler = configuration.ServerDataHandler;
            executor.ResponseHandler = configuration.ResponseHandler;
            executor.InstanceProvider = instanceProvider;

            return executor;
        }
    }
}
