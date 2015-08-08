namespace Bolt.Server.IntegrationTest.Core
{
    public interface ITestContractStateFull
    {
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
