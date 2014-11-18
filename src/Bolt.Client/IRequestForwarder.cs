using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IRequestForwarder
    {
        ResponseDescriptor<T> GetResponse<T, TParameters>(ClientExecutionContext context, TParameters parameters);

        Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(ClientExecutionContext context, TParameters parameters);
    }
}
