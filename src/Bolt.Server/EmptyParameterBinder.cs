using System.Threading.Tasks;

namespace Bolt.Server
{
    public class EmptyParameterBinder : IParameterBinder
    {
        public Task<BindingResult<T>> BindParametersAsync<T>(ServerActionContext context)
        {
            return Task.FromResult(BindingResult<T>.Error);
        }
    }
}