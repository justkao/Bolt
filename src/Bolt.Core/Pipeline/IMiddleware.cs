using System;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public interface IMiddleware<T> where T : ActionContextBase
    {
        Task Invoke(T context);

        void Init(ActionDelegate<T> next);

        void Validate(Type contract);
    }
}