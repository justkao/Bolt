using System.ServiceModel;
using System.Threading.Tasks;

namespace TestService.Core
{
    [ServiceContract]
    public interface IPersonRepositoryInner
    {
        void InnerOperation();

        Task InnerOperationExAsync();

    }
}