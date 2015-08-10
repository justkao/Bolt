using System;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class ReadResponseHandler : IClientContextHandler
    {
        public ReadResponseHandler(ISerializer serializer, IClientDataHandler dataHandler, IClientErrorProvider errorProvider)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (dataHandler == null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorProvider == null) throw new ArgumentNullException(nameof(errorProvider));

            Serializer = serializer;
            DataHandler = dataHandler;
            ErrorProvider = errorProvider;
        }

        public HandleContextStage Stage => HandleContextStage.After;

        public ISerializer Serializer { get; }

        public IClientDataHandler DataHandler { get; }

        public IClientErrorProvider ErrorProvider { get; }

        public virtual async Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            if (context.Response == null)
            {
                throw new BoltException($"Unable to process result for action '{context.Action.Name}' because response from server was not received.");
            }

            await HandleResponseAsync(context);
            await next(context);
        }

        protected virtual async Task HandleResponseAsync(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                BoltServerException boltError = ReadBoltServerErrorIfAvailable(context);
                if (boltError != null)
                {
                    throw boltError;
                }

                Exception errorOnServer = await DataHandler.ReadExceptionAsync(context);
                if (errorOnServer != null)
                {
                    context.ErrorResult = errorOnServer;
                    throw errorOnServer;
                }

                context.Response.EnsureSuccessStatusCode();
            }
            else
            {
                if (context.HasSerializableActionResult)
                {
                    context.ActionResult = await DataHandler.ReadResponseAsync(context);
                }
            }
        }

        protected virtual BoltServerException ReadBoltServerErrorIfAvailable(ClientActionContext context)
        {
            return ErrorProvider.TryReadServerError(context);
        }

    }
}
