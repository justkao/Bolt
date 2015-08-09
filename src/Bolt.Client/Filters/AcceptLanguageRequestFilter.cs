using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bolt.Client.Filters
{
    public class AcceptLanguageExecutionFilter : IClientExecutionFilter
    {
        public Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            context.Request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.CurrentCulture.Name));
            return next(context);
        }
    }
}
