using System.ServiceModel;
using System.Threading.Tasks;

namespace Bolt.Performance.Contracts
{
    [ServiceContract]
    public interface IInnerTestContract2
    {
        [AsyncOperation]
        void InnerOperation2();

        Task InnerOperationExAsync2();
    }
}