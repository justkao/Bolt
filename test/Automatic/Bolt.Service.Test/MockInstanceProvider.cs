using System;
using Bolt.Server;

namespace Bolt.Service.Test
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public T GetInstance<T>(ServerActionContext context)
        {
            return (T)CurrentInstance;
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
        }
    }
}