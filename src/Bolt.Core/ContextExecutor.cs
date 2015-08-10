using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt
{
    public class ContextExecutor<T> : IContextHandler<T> where T : ActionContextBase
    {
        private readonly IReadOnlyCollection<IContextHandler<T>> _handlers;
        private T _context;
        private IEnumerator<IContextHandler<T>> _currentFilter;
        private bool _handled;

        public ContextExecutor(IReadOnlyCollection<IContextHandler<T>> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _handlers = handlers;
        }

        public HandleContextStage Stage  => HandleContextStage.Execute; 

        public async Task HandleAsync(T context, Func<T, Task> next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (_handled)
            {
                throw new InvalidOperationException($"{0} instance was already used. Create new instance to handle context again.");
            }

            _handled = true;
            _context = context;
            _currentFilter = _handlers.GetEnumerator();
            await ExecuteInternalAsync(_context);
        }

        private async Task ExecuteInternalAsync(T context)
        {
            if (_currentFilter.MoveNext())
            {
                await _currentFilter.Current.HandleAsync(context, ExecuteInternalAsync);
            }
        }
    }
}