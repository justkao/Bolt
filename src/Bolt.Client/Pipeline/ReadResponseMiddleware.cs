using System;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class ReadResponseMiddleware : ClientMiddlewareBase
    {
        public ReadResponseMiddleware(ActionDelegate<ClientActionContext> next, ISerializer serializer,
            IClientDataHandler dataHandler, IClientErrorProvider errorProvider) : base(next)
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

        public override async Task Invoke(ClientActionContext context)
        {
            if (context.Response == null)
            {
                throw new BoltException($"Unable to process result for action '{context.Action.Name}' because response from server was not received.");
            }

            await HandleResponseAsync(context);
            await Next(context);
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
