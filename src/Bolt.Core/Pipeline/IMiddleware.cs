using System;
using System.Threading.Tasks;
using Bolt.Metadata;

namespace Bolt.Pipeline
{
    public interface IMiddleware<T> where T : ActionContextBase
    {
        Task InvokeAsync(T context);

        void Init(ActionDelegate<T> next);

        void Validate(ContractMetadata contract);
    }
}