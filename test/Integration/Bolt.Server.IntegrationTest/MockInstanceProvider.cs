using System;
using System.Threading.Tasks;

namespace Bolt.Server.IntegrationTest
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public object GetInstanceAsync(ServerActionContext context, Type type)
        {
            return CurrentInstance;
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
        }

        public Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            throw new NotImplementedException();
        }

        Task<object> IInstanceProvider.GetInstanceAsync(ServerActionContext context, Type type)
        {
            throw new NotImplementedException();
        }
    }
}