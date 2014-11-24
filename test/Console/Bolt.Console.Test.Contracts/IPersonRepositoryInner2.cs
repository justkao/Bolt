using System.Threading.Tasks;

namespace Bolt.Console.Test.Contracts
{
    public interface IPersonRepositoryInner2
    {
        [AsyncOperation]
        void InnerOperation2();

        Task InnerOperationExAsync2();
    }
}