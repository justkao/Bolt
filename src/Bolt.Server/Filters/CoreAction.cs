using Bolt.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Server.Filters
{
    public class CoreAction : IActionExecutionFilter
    {
        private IReadOnlyCollection<IActionExecutionFilter> _filters;
        private ServerActionContext _context;
        private Func<ServerActionContext, Task> _coreAction;
        private IEnumerator<IActionExecutionFilter> _currentFilter;

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

        protected virtual IEnumerable<IActionExecutionFilter> GetFilters(ServerActionContext context)
        {
            return
                context.HttpContext.GetFeature<IBoltFeature>().FilterProviders.EmptyIfNull()
                    .SelectMany(f => f.GetFilters(context))
                    .OrderBy(f => f.Order)
                    .ToList();
        }

        protected virtual async Task ExecuteCore(ServerActionContext context)
        {
            var feature = context.HttpContext.GetFeature<IBoltFeature>();

            if (context.Action.HasParameters && context.Parameters == null)
            {
                context.Parameters =
                    feature.Configuration.Serializer.DeserializeParameters(
                        await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted), context.Action);
            }

            bool instanceCreated = false;
            if (context.ContractInstance == null)
            {
                context.ContractInstance = context.ContractInvoker.InstanceProvider.GetInstance(context, context.Action.Contract.Type);
                instanceCreated = true;
            }

            try
            {
                await _coreAction(context);
                context.IsExecuted = true;

                if (instanceCreated)
                {
                    ReleaseInstanceSafe(context, null);
                }
            }
            catch (Exception e)
            {
                if (instanceCreated)
                {
                    ReleaseInstanceSafe(context, e);
                }
                throw;
            }
        }

        private void ReleaseInstanceSafe(ServerActionContext context, Exception exception)
        {
            try
            {
                context.ContractInvoker.InstanceProvider.ReleaseInstance(context, context.ContractInstance, exception);
            }
            catch (Exception)
            {
                // TODO: log ? 
            }
            finally
            {
                context.ContractInstance = null;
            }
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