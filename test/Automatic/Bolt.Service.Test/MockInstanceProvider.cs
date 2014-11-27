using Bolt.Server;

namespace Bolt.Service.Test
{
    public class MockInstanceProvider : IInstanceProvider
    {
        public object CurrentInstance { get; set; }

        public T GetInstance<T>(ServerExecutionContext context)
        {
            return (T)CurrentInstance;
        }
    }
}