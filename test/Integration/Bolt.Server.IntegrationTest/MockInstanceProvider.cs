using System;
using Bolt.Server;

namespace Bolt.Server.IntegrationTest
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public object GetInstance(ServerActionContext context, Type type)
        {
            return CurrentInstance;
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
        }
    }
}