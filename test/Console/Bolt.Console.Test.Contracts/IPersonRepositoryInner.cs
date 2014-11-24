using System.Threading.Tasks;

namespace Bolt.Console.Test.Contracts
{
    public interface IPersonRepositoryInner
    {
        [AsyncOperation]
        void InnerOperation();

        Task InnerOperationExAsync();
    }
}