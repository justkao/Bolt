using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public class PipelineBuilder<T> where T : ActionContextBase
    {
        private readonly IList<IMiddleware<T>> _middlewares = new List<IMiddleware<T>>();

        public IEnumerable<IMiddleware<T>> Middlewares => _middlewares;

        public PipelineBuilder<T> Use(Func<ActionDelegate<T>, T, Task> action)
        {
            return Use(new DelegatedMiddleware<T>(action));
        } 

        public PipelineBuilder<T> Use(IMiddleware<T> middleware)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            _middlewares.Add(middleware);
            return this;
        }

        public virtual PipelineResult<T> Build()
        {
            return new PipelineResult<T>(BuildActionDelegate(), (List<IMiddleware<T>>)_middlewares);
        }

        protected ActionDelegate<T> BuildActionDelegate()
        {
            ActionDelegate<T> app = context => Task.FromResult(0);

            foreach (var middleware in _middlewares.Reverse())
            {
                Func<ActionDelegate<T>, ActionDelegate<T>> actionDelegate = next =>
                    {
                        middleware.Init(next);
                        return middleware.InvokeAsync;
                    };

                app = actionDelegate(app);
            }

            return app;
        }
    }
}
