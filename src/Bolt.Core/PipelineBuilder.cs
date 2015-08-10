using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.Internal;

namespace Bolt.Core
{
    public class PipelineBuilder<T> where T : ActionContextBase
    {
        private readonly IList<Func<ActionDelegate<T>, ActionDelegate<T>>> _middlewares =
            new List<Func<ActionDelegate<T>, ActionDelegate<T>>>();

        public PipelineBuilder(IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            Services = services;
        }

        public PipelineBuilder<T> Use(Func<ActionDelegate<T>, ActionDelegate<T>> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        public IServiceProvider Services { get; }

        public PipelineBuilder<T> Use<TMiddleware>(params object[] args) where TMiddleware : IMiddleware<T>
        {
            return Use(next =>
            {
                Type middleWare = typeof (TMiddleware);
                MethodInfo methodinfo = middleWare.GetRuntimeMethod(nameof(IMiddleware<T>.Invoke), new[] {typeof (T)});
                object instance = ActivatorUtilities.CreateInstance(Services, middleWare, new[] {next}.Concat(args).ToArray());
                return (ActionDelegate<T>) methodinfo.CreateDelegate(typeof (ActionDelegate<T>), instance);
            });
        }

        public ActionDelegate<T> Build()
        {
            ActionDelegate<T> app = context => Task.FromResult(0);
            foreach (var component in _middlewares.Reverse())
            {
                app = component(app);
            }

            return app;
        }
    }
}
