using System;
using System.IO;
using System.Threading.Tasks;
using Bolt.Session;

namespace Bolt.Server
{
    public class SessionHandler : ISessionHandler
    {
        public async Task HandleInitSessionAsync(ServerActionContext context)
        {
            IBoltFeature feature = context.HttpContext.GetFeature<IBoltFeature>();
            ISessionCallback callback = context.ContractInstance as ISessionCallback;
            if (callback == null)
            {
                context.Result = new InitSessionResult();
                return;
            }

            InitSessionParameters parameters;
            try
            {
                parameters = feature.Configuration.Serializer.Read<InitSessionParameters>(await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted));
            }
            catch (Exception e)
            {
                throw new DeserializeParametersException(
                    $"Failed to deserialize init session parameters for contract {BoltFramework.GetContractName(context.Contract)}.",
                    e);
            }

            context.Result = await callback.InitSessionAsync(parameters, context.RequestAborted);
        }

        public async Task HandleDestroySessionAsync(ServerActionContext context)
        {
            IBoltFeature feature = context.HttpContext.GetFeature<IBoltFeature>();
            ISessionCallback callback = context.ContractInstance as ISessionCallback;
            if (callback == null)
            {
                context.Result = new InitSessionResult();
                return;
            }

            DestroySessionParameters parameters;
            try
            {
                parameters = feature.Configuration.Serializer.Read<DestroySessionParameters>(await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted));
            }
            catch (Exception e)
            {
                throw new DeserializeParametersException(
                    $"Failed to deserialize destroy session parameters for contract {BoltFramework.GetContractName(context.Contract)}.",
                    e);
            }

            context.Result = await callback.DestroySessionAsync(parameters, context.RequestAborted);
        }
    }
}