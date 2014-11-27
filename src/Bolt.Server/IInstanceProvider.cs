namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        T GetInstance<T>(ServerExecutionContext context);

        void ReleaseInstance(ServerExecutionContext context, object obj);
    }
}