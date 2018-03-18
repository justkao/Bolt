using System;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class InstanceProviderMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task InvokeAsync(ServerActionContext context)
        {
            bool instanceCreated = false;
            if (context.ContractInstance == null)
            {
                context.ContractInstance = await context.ContractInvoker.InstanceProvider.GetInstanceAsync(context, context.ContractInvoker.Contract.Contract);
                instanceCreated = true;
            }

            try
            {
                await Next(context);
            }
            catch (Exception e)
            {
                if (instanceCreated)
                {
                    await ReleaseInstanceSafeAsync(context, e, false);
                }

                throw;
            }

            if (instanceCreated)
            {
                await ReleaseInstanceSafeAsync(context, null, true);
            }
        }

        private async Task ReleaseInstanceSafeAsync(ServerActionContext context, Exception exception, bool throwError)
        {
            try
            {
                await context.ContractInvoker.InstanceProvider.ReleaseInstanceAsync(context, context.ContractInstance, exception);
            }
            catch (Exception)
            {
                if (throwError)
                {
                    throw;
                }
            }
            finally
            {
                context.ContractInstance = null;
            }
        }
    }
}
