using System.IO;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ParameterHandler : IParameterHandler
    {
        public async Task HandleAsync(ServerActionContext context)
        {
            if (context.Action.HasParameters && context.Parameters == null)
            {
                var feature = context.HttpContext.GetFeature<IBoltFeature>();

                context.Parameters =
                    feature.Configuration.Serializer.DeserializeParameters(
                        await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted),
                        context.Action);
            }
        }
    }
}