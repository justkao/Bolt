using System.Threading.Tasks;

using Bolt.Pipeline;

namespace Bolt.Client.Test
{
    public interface ITestContract
    {
        string Execute(string param);

        Task ExecuteAsync();
    }

    public class TestContractProxy : ProxyBase, ITestContract
    {
        public TestContractProxy(IPipeline<ClientActionContext> pipeline)
            : base(typeof(ITestContract), pipeline)
        {
        }

        public string Execute(string param)
        {
            return this.Send<string>(Contract.GetMethod(nameof(Execute)), param);
        }

        public Task ExecuteAsync()
        {
            return SendAsync(Contract.GetMethod(nameof(ExecuteAsync)));
        }
    }
}