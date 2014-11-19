using Bolt;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestService.Core
{
    [ServiceContract]
    public interface IPersonRepositoryInner2
    {
        [AsyncOperation]
        void InnerOperation2();

        Task InnerOperationExAsync2();
    }

    [ServiceContract]
    public interface IPersonRepositoryInner
    {
        [AsyncOperation]
        void InnerOperation();

        Task InnerOperationExAsync();
    }
}