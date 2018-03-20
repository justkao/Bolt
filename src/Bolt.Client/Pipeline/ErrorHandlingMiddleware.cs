using System;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class ErrorHandlingMiddleware : ClientMiddlewareBase
    {
        private readonly IErrorHandling _errorHandling;

        public ErrorHandlingMiddleware(IErrorHandling errorHandling)
        {
            _errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));
        }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            try
            {
                await Next(context).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ErrorHandlingResult result = _errorHandling.Handle(context, e);
                if (result == ErrorHandlingResult.Close || result == ErrorHandlingResult.Recover)
                {
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                }

                throw;
            }
        }
    }
}