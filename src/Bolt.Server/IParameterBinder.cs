using System.Threading.Tasks;

namespace Bolt.Server
{
    public class BindingResult<T>
    {
        public static readonly BindingResult<T> Empty = new BindingResult<T>();

        private BindingResult()
        {
            Success = false;
        }

        public BindingResult(T parameters)
        {
            Parameters = parameters;
            Success = true;
        }

        public T Parameters { get; private set; }

        public bool Success { get; private set; }
    }

    public interface IParameterBinder
    {
        Task<BindingResult<T>> BindParametersAsync<T>(ServerActionContext context);
    }
}