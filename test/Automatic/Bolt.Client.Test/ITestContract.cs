using System.Threading.Tasks;
using Bolt.Client.Pipeline;

namespace Bolt.Client.Test
{
    public interface ITestContract
    {
        [InitSession]
        Task<string> OpenSession(string param);

        [DestroySession]
        Task<string> CloseSession(string param);

        string Execute(string param);

        Task ExecuteAsync();
    }

    public class TestContractProxy : ProxyBase, ITestContract
    {
        public TestContractProxy(IClientPipeline pipeline)
            : base(typeof(ITestContract), pipeline)
        {
        }

        public Task<string> OpenSession(string param)
        {
            return this.SendAsync<string>(Contract.GetMethod(nameof(OpenSession)), param);
        }

        public Task<string> CloseSession(string param)
        {
            return this.SendAsync<string>(Contract.GetMethod(nameof(CloseSession)), param);
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