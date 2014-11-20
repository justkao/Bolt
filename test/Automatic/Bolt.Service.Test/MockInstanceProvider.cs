using System.Threading.Tasks;
using Bolt.Server;

namespace Bolt.Service.Test
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public async Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            return (T)CurrentInstance;
        }
    }
}