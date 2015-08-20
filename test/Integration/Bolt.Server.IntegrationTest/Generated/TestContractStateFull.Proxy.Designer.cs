//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.

//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client;
using Bolt.Server.IntegrationTest.Core;


namespace Bolt.Server.IntegrationTest.Core
{
    public partial interface ITestContractStateFullAsync : ITestContractStateFull
    {
        Task SetStateAsync(string state);

        Task<string> GetStateAsync();

        Task NextCallWillFailProxyAsync();

        Task<string> GetSessionIdAsync();
    }
}

namespace Bolt.Server.IntegrationTest.Core
{
    public partial class TestContractStateFullProxy : Bolt.Client.ProxyBase, Bolt.Server.IntegrationTest.Core.ITestContractStateFull, ITestContractStateFullAsync
    {
        public TestContractStateFullProxy(Bolt.Server.IntegrationTest.Core.TestContractStateFullProxy proxy) : base(proxy)
        {
        }

        public TestContractStateFullProxy(Bolt.Client.Pipeline.IClientPipeline channel) : base(typeof(Bolt.Server.IntegrationTest.Core.ITestContractStateFull), channel)
        {
        }

        public virtual Task<string> OpenSessionAsync(string arguments)
        {
            return this.SendAsync<string>(__OpenSessionAsyncAction, arguments);
        }

        public virtual void SetState(string state)
        {
            this.Send(__SetStateAction, state);
        }

        public virtual Task SetStateAsync(string state)
        {
            return this.SendAsync(__SetStateAction, state);
        }

        public virtual string GetState()
        {
            return this.Send<string>(__GetStateAction);
        }

        public virtual Task<string> GetStateAsync()
        {
            return this.SendAsync<string>(__GetStateAction);
        }

        public virtual void NextCallWillFailProxy()
        {
            this.Send(__NextCallWillFailProxyAction);
        }

        public virtual Task NextCallWillFailProxyAsync()
        {
            return this.SendAsync(__NextCallWillFailProxyAction);
        }

        public virtual string GetSessionId()
        {
            return this.Send<string>(__GetSessionIdAction);
        }

        public virtual Task<string> GetSessionIdAsync()
        {
            return this.SendAsync<string>(__GetSessionIdAction);
        }


        private static readonly MethodInfo __OpenSessionAsyncAction = typeof(ITestContractStateFull).GetMethod(nameof(ITestContractStateFull.OpenSessionAsync));
        private static readonly MethodInfo __SetStateAction = typeof(ITestContractStateFull).GetMethod(nameof(ITestContractStateFull.SetState));
        private static readonly MethodInfo __GetStateAction = typeof(ITestContractStateFull).GetMethod(nameof(ITestContractStateFull.GetState));
        private static readonly MethodInfo __NextCallWillFailProxyAction = typeof(ITestContractStateFull).GetMethod(nameof(ITestContractStateFull.NextCallWillFailProxy));
        private static readonly MethodInfo __GetSessionIdAction = typeof(ITestContractStateFull).GetMethod(nameof(ITestContractStateFull.GetSessionId));
    }
}