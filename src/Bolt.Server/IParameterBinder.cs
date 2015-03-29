using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IParameterBinder
    {
        Task<T> BindParametersAsync<T>(ServerActionContext context);
    }
}