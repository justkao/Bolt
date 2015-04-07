using System.Threading.Tasks;

namespace Bolt.Server
{
    public class EmptyParameterBinder : IParameterBinder
    {
        public Task<BindingResult<T>> BindParametersAsync<T>(ServerActionContext context)
        {
            return CachedBindingResult<T>.Instance;
        }

        private static class CachedBindingResult<T>
        {
            public static readonly Task<BindingResult<T>> Instance = Task.FromResult(BindingResult<T>.Empty);
        }
    }
}