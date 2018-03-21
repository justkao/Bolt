using System;
using System.Reflection;
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

        void DoNothing();

        Task DoNothingAsync();

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
            return this.SendAsync<string>(GetMethod(nameof(OpenSession), typeof(string)), param);
        }

        public Task<string> CloseSession(string param)
        {
            return this.SendAsync<string>(GetMethod(nameof(CloseSession), typeof(string)), param);
        }

        public string Execute(string param)
        {
            return this.Send<string>(GetMethod(nameof(Execute), typeof(string)), param);
        }

        public void DoNothing()
        {
            this.Send<string>(GetMethod(nameof(DoNothingAsync)));
        }

        public Task DoNothingAsync()
        {
            return this.SendAsync<string>(GetMethod(nameof(DoNothingAsync)));
        }

        public Task ExecuteAsync()
        {
            return SendAsync(GetMethod(nameof(ExecuteAsync)));
        }

        private MethodInfo GetMethod(string name, params Type[] paramters)
        {
            return Contract.Contract.GetTypeInfo().GetMethod(name, paramters);
        }
    }
}