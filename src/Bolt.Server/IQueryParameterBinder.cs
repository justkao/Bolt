namespace Bolt.Server
{
    public interface IQueryParameterBinder
    {
        T BindParameters<T>(ServerActionContext context);
    }
}