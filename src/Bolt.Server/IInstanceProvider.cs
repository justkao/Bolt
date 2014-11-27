namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        T GetInstance<T>(ServerExecutionContext context);
    }
}