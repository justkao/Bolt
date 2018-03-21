using System.Threading.Tasks;

namespace Bolt.Server.IntegrationTest.Core
{
    public interface ITestContractStateFull
    {
        [AsyncOperation]
        [InitSession]
        Task<string> OpenSessionAsync(string arguments);

        [AsyncOperation]
        void SetState(string state);

        [AsyncOperation]
        string GetState();

        [AsyncOperation]
        void NextCallWillFailProxy();

        [AsyncOperation]
        string GetSessionId();
    }
}
