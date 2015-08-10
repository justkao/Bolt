using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt
{
    public abstract class PipelineContextHandler<T> : IContextHandler<T> where T : ActionContextBase
    {
        private readonly IReadOnlyCollection<IContextHandler<T>> _handlers;
        private T _context;
        private IEnumerator<IContextHandler<T>> _currentFilter;
        private bool _handled;

        protected PipelineContextHandler(IReadOnlyCollection<IContextHandler<T>> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _handlers = handlers;
        }

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

            if (!_handlers.Any())
            {
                await ExecuteCoreAsync(context);
            }
            else
            {
                _currentFilter = _handlers.GetEnumerator();
                await ExecuteInternalAsync(_context);
            }
        }

        private async Task ExecuteInternalAsync(T context)
        {
            if (!_currentFilter.MoveNext())
            {
                await ExecuteCoreAsync(_context);
            }
            else
            {
                await _currentFilter.Current.HandleAsync(context, ExecuteInternalAsync);
            }
        }

        protected abstract Task ExecuteCoreAsync(T context);
    }
}