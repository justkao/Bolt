using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Pipeline
{
    public class PipelineResult<T> : IPipeline<T> where T : ActionContextBase
    {
        public PipelineResult(ActionDelegate<T> pipeline, IReadOnlyCollection<IMiddleware<T>> middlewares)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (middlewares == null) throw new ArgumentNullException(nameof(middlewares));

            Instance = pipeline;
            Middlewares = middlewares;
        }

        public ActionDelegate<T> Instance { get; }

        public TMiddleware Find<TMiddleware>() where TMiddleware : IMiddleware<T>
        {
            return (TMiddleware)Middlewares.FirstOrDefault(m => m is TMiddleware);
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