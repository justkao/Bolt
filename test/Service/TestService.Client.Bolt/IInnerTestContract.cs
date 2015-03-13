using System.Threading.Tasks;
using Bolt;

namespace TestService.Core
{
    public interface IInnerTestContract2
    {
        [AsyncOperation]
        void InnerOperation2();

        Task InnerOperationExAsync2();
    }

    public interface IInnerTestContract
    {
        [AsyncOperation]
        void InnerOperation();

        Task InnerOperationExAsync();
    }
}