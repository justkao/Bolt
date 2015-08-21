using System.ServiceModel;
using System.Threading.Tasks;

namespace Bolt.Performance.Contracts
{
    [ServiceContract]
    public interface IInnerTestContract
    {
        [AsyncOperation]
        void InnerOperation();

        Task<string> InnerOperation3();

        Task InnerOperationExAsync();
    }
}