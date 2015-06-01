using System;
using System.Threading.Tasks;

namespace Bolt.Server.IntegrationTest
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            return Task.FromResult(true);
        }

        Task<object> IInstanceProvider.GetInstanceAsync(ServerActionContext context, Type type)
        {
            return Task.FromResult(CurrentInstance);
        }
    }
}