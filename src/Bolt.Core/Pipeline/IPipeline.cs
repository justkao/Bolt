using System;
using Bolt.Metadata;

namespace Bolt.Pipeline
{
    public interface IPipeline<T> : IDisposable where T : ActionContextBase
    {
        ActionDelegate<T> Instance { get; }

        TMiddleware Find<TMiddleware>() where TMiddleware : IMiddleware<T>;

        void Validate(ContractMetadata contract);
    }
}