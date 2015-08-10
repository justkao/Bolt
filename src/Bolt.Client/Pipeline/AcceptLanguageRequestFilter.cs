using System.Globalization;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class AcceptLanguageMiddleware : ClientMiddlewareBase
    {
        public AcceptLanguageMiddleware(ActionDelegate<ClientActionContext> next) : base(next)
        {
        }

        public override Task Invoke(ClientActionContext context)
        {
            context.Request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.CurrentCulture.Name));
            return Next(context);
        }
    }
}
