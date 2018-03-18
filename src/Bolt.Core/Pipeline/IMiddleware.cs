using Bolt.Metadata;
using System;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public interface IMiddleware<T> where T : ActionContextBase
    {
        Task InvokeAsync(T context);

        void Init(ActionDelegate<T> next);

        void Validate(ContractMetadata contract);
    }
}