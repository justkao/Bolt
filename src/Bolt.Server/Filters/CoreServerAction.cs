using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Bolt.Common;

namespace Bolt.Server.Filters
{
    public class CoreServerAction : IServerExecutionFilter
    {
        private IReadOnlyCollection<IServerExecutionFilter> _filters;
        private ServerActionContext _context;
        private Func<ServerActionContext, Task> _coreAction;
        private IEnumerator<IServerExecutionFilter> _currentFilter;

        public int Order => int.MaxValue;

        public async Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> coreAction)
        {
            _context = context;
            _coreAction = coreAction;
            _filters = GetFilters(context).ToList();

            if (!_filters.Any())
            {
                await ExecuteCore(_context);
            }
            else
            {
                _currentFilter = _filters.GetEnumerator();
                await ExecuteAsync(_context);
            }
        }

        protected virtual IEnumerable<IServerExecutionFilter> GetFilters(ServerActionContext context)
        {
            return
                context.HttpContext.GetFeature<IBoltFeature>().FilterProviders.EmptyIfNull()
                    .SelectMany(f => f.GetFilters(context))
                    .OrderBy(f => f.Order)
                    .ToList();
        }

        protected virtual async Task ExecuteCore(ServerActionContext context)
        {
            bool instanceCreated = false;
            if (context.ContractInstance == null)
            {
                context.ContractInstance = await context.ContractInvoker.InstanceProvider.GetInstanceAsync(context, context.ContractInvoker.Contract);
                instanceCreated = true;
            }

            try
            {
                await _coreAction(context);
                context.IsExecuted = true;

                if (instanceCreated)
                {
                    await ReleaseInstanceSafeAsync(context, null);
                }
            }
            catch (Exception e)
            {
                if (instanceCreated)
                {
                    await ReleaseInstanceSafeAsync(context, e);
                }
                throw;
            }
        }

        private Task ReleaseInstanceSafeAsync(ServerActionContext context, Exception exception)
        {
            try
            {
                return context.ContractInvoker.InstanceProvider.ReleaseInstanceAsync(context, context.ContractInstance, exception);
            }
            catch (Exception)
            {
                // TODO: log ? 
            }
            finally
            {
                context.ContractInstance = null;
            }

            return CompletedTask.Done;
        }

        private async Task ExecuteAsync(ServerActionContext context)
        {
            if (context.IsExecuted)
            {
                return;
            }

            if (!_currentFilter.MoveNext())
            {
                await ExecuteCore(_context);
            }
            else
            {
                await _currentFilter.Current.ExecuteAsync(context, ExecuteAsync);
            }
        }
    }
}