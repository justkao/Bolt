using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public class PipelineBuilder<T> where T : ActionContextBase
    {
        private readonly IList<Func<ActionDelegate<T>, ActionDelegate<T>>> _middlewares = new List<Func<ActionDelegate<T>, ActionDelegate<T>>>();
        private List<IMiddleware<T>> _instances = new List<IMiddleware<T>>(); 

        public PipelineBuilder<T> Use(IMiddleware<T> middleware)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            _instances.Add(middleware);

            return Use(next =>
            {
                middleware.Init(next);
                return middleware.Invoke;
            });
        }

        public PipelineResult<T> Build()
        {
            ActionDelegate<T> app = context => Task.FromResult(0);
            foreach (var component in _middlewares.Reverse())
            {
                app = component(app);
            }

            return new PipelineResult<T>(app, _instances);
        }

        private PipelineBuilder<T> Use(Func<ActionDelegate<T>, ActionDelegate<T>> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }
    }
}
