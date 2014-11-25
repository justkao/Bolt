using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IRequestForwarder
    {
        ResponseDescriptor<T> GetResponse<T, TParameters>(ClientActionContext context, TParameters parameters);

        Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(ClientActionContext context, TParameters parameters);
    }
}
