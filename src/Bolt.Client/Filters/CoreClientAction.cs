using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Client.Filters
{
    public class CoreClientAction : IClientExecutionFilter
    {
        private readonly IReadOnlyCollection<IClientExecutionFilter> _filters;
        private ClientActionContext _context;
        private Func<ClientActionContext, Task> _coreAction;
        private IEnumerator<IClientExecutionFilter> _currentFilter;

        public int Order => int.MaxValue;

        public CoreClientAction(IReadOnlyCollection<IClientExecutionFilter> filters)
        {
            _filters = filters;
        }

        public async Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> coreAction)
        {
            _context = context;
            _coreAction = coreAction;

            if (!_filters.Any())
            {
                await _coreAction(context);
            }
            else
            {
                _currentFilter = _filters.GetEnumerator();
                await ExecuteAsync(_context);
            }
        }

        private async Task ExecuteAsync(ClientActionContext context)
        {
            if (!_currentFilter.MoveNext())
            {
                await _coreAction(_context);
            }
            else
            {
                await _currentFilter.Current.ExecuteAsync(context, ExecuteAsync);
            }
        }
    }
}