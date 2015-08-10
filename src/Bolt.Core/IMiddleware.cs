using System.Threading.Tasks;

namespace Bolt.Core
{
    public interface IMiddleware<T> where T : ActionContextBase
    {
        Task Invoke(T context);
    }
}