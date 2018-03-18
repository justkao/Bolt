using Bolt.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Pipeline
{
    public class PipelineResult<T> : IPipeline<T> where T : ActionContextBase
    {
        public PipelineResult(ActionDelegate<T> pipeline, IReadOnlyCollection<IMiddleware<T>> middlewares)
        {
            Instance = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            Middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
        }

        public ActionDelegate<T> Instance { get; }

        public TMiddleware Find<TMiddleware>() where TMiddleware : IMiddleware<T>
        {
            return (TMiddleware)Middlewares.FirstOrDefault(m => m is TMiddleware);
        }

        public void Validate(ContractMetadata contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            foreach (IMiddleware<T> middleware in Middlewares)
            {
                middleware.Validate(contract);
            }
        }

        public IReadOnlyCollection<IMiddleware<T>> Middlewares { get; }

        public void Dispose()
        {
            foreach (IDisposable disposable in Middlewares.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}