using Bolt.Session;

namespace Bolt.Server.IntegrationTest
{
    public interface ITestState
    {
        Moq.Mock<ISessionCallback> SessionCallback { get; set; } 
    }
}