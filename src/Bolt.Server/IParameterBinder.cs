using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IParameterBinder
    {
        Task<BindingResult> BindParametersAsync(ServerActionContext context);
    }
}