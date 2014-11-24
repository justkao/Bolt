using Bolt.Server;
using System.Threading.Tasks;

namespace Bolt.Service.Test
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            return Task.FromResult((T)CurrentInstance);
        }
    }
}