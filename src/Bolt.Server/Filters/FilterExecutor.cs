using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Server.Filters
{
    internal class FilterExecutor
    {
        private readonly IReadOnlyCollection<IActionExecutionFilter> _filters;
        private readonly ServerActionContext _context;
        private readonly Func<ServerActionContext, Task> _boltExecution;
        private IEnumerator<IActionExecutionFilter> _currentFilter;

        public FilterExecutor(IReadOnlyCollection<IActionExecutionFilter> filters, ServerActionContext context, Func<ServerActionContext, Task> boltExecution)
        {
            _filters = filters;
            _context = context;
            _boltExecution = boltExecution;
        }

        public async Task ExecuteAsync()
        {
            if (_filters == null || !_filters.Any())
            {
                try
                {
                    await _boltExecution(_context);
                }
                finally
                {
                    _context.Executed = true;
                }
            }
            else
            {
                _currentFilter = _filters.GetEnumerator();
                await ExecuteAsync(_context);
            }
        }

        private async Task ExecuteAsync(ServerActionContext context)
        {
            if (context.Executed)
            {
                return;
            }

            if (!_currentFilter.MoveNext())
            {
                try
                {
                    await _boltExecution(context);
                }
                finally
                {
                    context.Executed = true;
                }
            }
            else
            {
                await _currentFilter.Current.ExecuteAsync(context, ExecuteAsync);
            }
        }
    }
}