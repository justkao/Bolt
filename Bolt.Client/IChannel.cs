using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IChannel
    {
        Task ExecuteAsync<TRequestParameters>(TRequestParameters parameters, string method);

        Task<TResult> ExecuteAsync<TResult, TRequestParameters>(TRequestParameters parameters, string method);

        void Execute<TRequestParameters>(TRequestParameters parameters, string method);

        TResult Execute<TResult, TRequestParameters>(TRequestParameters parameters, string method);
    }
}