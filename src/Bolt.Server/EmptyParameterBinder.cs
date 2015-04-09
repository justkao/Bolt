using System.Threading.Tasks;

namespace Bolt.Server
{
    public class EmptyParameterBinder : IParameterBinder
    {
        public Task<BindingResult> BindParametersAsync(ServerActionContext context)
        {
            return CachedBindingResult.Instance;
        }

        private static class CachedBindingResult
        {
            public static readonly Task<BindingResult> Instance = Task.FromResult(BindingResult.Empty);
        }
    }
}