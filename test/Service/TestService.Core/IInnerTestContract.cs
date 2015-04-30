using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestService.Core
{
    public class AsyncOperationAttribute : Attribute
    {
    }

    [ServiceContract]
    public interface IInnerTestContract2
    {
        [AsyncOperation]
        void InnerOperation2();

        Task InnerOperationExAsync2();
    }

    [ServiceContract]
    public interface IInnerTestContract
    {
        [AsyncOperation]
        void InnerOperation();

        Task<string> InnerOperation3();

        Task InnerOperationExAsync();
    }
}