﻿using Bolt;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestService.Core
{
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

        Task InnerOperationExAsync();
    }
}