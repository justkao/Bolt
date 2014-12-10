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
using System.Text;
using System.Threading.Tasks;

using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Service.Test.Core;
using Bolt.Service.Test.Core.Parameters;


namespace Bolt.Service.Test.Core
{
    public partial interface ITestContractStateFullAsync : ITestContractStateFull
    {
        Task InitAsync();

        Task SetStateAsync(string state);

        Task<string> GetStateAsync();

        Task NextCallWillFailProxyAsync();

        Task DestroyAsync();
    }
}

namespace Bolt.Service.Test.Core
{
    public partial class TestContractStateFullProxy : ContractProxy<Bolt.Service.Test.Core.TestContractStateFullDescriptor>, Bolt.Service.Test.Core.ITestContractStateFull, ITestContractStateFullAsync
    {
        // useless comment added by user generator - 'Bolt.Service.Test.Core.UserCodeGenerator', Context - ''

        public TestContractStateFullProxy(Bolt.Service.Test.Core.TestContractStateFullProxy proxy) : base(proxy)
        {
        }

        public TestContractStateFullProxy(IChannel channel) : base(channel)
        {
        }

        public virtual void Init()
        {
            Channel.Send(Bolt.Empty.Instance, Descriptor.Init, GetCancellationToken(Descriptor.Init));
        }

        public virtual Task InitAsync()
        {
            return Channel.SendAsync(Bolt.Empty.Instance, Descriptor.Init, GetCancellationToken(Descriptor.Init));
        }

        public virtual void SetState(string state)
        {
            var bolt_Params = new Bolt.Service.Test.Core.Parameters.SetStateParameters();
            bolt_Params.State = state;
            Channel.Send(bolt_Params, Descriptor.SetState, GetCancellationToken(Descriptor.SetState));
        }

        public virtual Task SetStateAsync(string state)
        {
            var bolt_Params = new Bolt.Service.Test.Core.Parameters.SetStateParameters();
            bolt_Params.State = state;
            return Channel.SendAsync(bolt_Params, Descriptor.SetState, GetCancellationToken(Descriptor.SetState));
        }

        public virtual string GetState()
        {
            return Channel.Send<string, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.GetState, GetCancellationToken(Descriptor.GetState));
        }

        public virtual Task<string> GetStateAsync()
        {
            return Channel.SendAsync<string, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.GetState, GetCancellationToken(Descriptor.GetState));
        }

        public virtual void NextCallWillFailProxy()
        {
            Channel.Send(Bolt.Empty.Instance, Descriptor.NextCallWillFailProxy, GetCancellationToken(Descriptor.NextCallWillFailProxy));
        }

        public virtual Task NextCallWillFailProxyAsync()
        {
            return Channel.SendAsync(Bolt.Empty.Instance, Descriptor.NextCallWillFailProxy, GetCancellationToken(Descriptor.NextCallWillFailProxy));
        }

        public virtual void Destroy()
        {
            Channel.Send(Bolt.Empty.Instance, Descriptor.Destroy, GetCancellationToken(Descriptor.Destroy));
        }

        public virtual Task DestroyAsync()
        {
            return Channel.SendAsync(Bolt.Empty.Instance, Descriptor.Destroy, GetCancellationToken(Descriptor.Destroy));
        }

    }
}