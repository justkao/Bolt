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

using Bolt.Service.Test.Core;
using Bolt.Service.Test.Core.Parameters;


namespace Bolt.Service.Test.Core
{
    public partial interface ITestContractStateFullAsync : ITestContractStateFull
    {
        Task<string> GetStateAsync();
    }
}

namespace Bolt.Service.Test.Core
{
    public partial class TestContractStateFullProxy : Bolt.Client.Channels.ContractProxy<Bolt.Service.Test.Core.TestContractStateFullDescriptor>, Bolt.Service.Test.Core.ITestContractStateFull, ITestContractStateFullAsync
    {
        public TestContractStateFullProxy(Bolt.Service.Test.Core.TestContractStateFullProxy proxy) : base(proxy)
        {
        }

        public TestContractStateFullProxy(Bolt.Client.IChannel channel) : base(channel)
        {
        }

        public virtual void Init()
        {
            Channel.Send(Bolt.Empty.Instance, Descriptor.Init, GetCancellationToken(Descriptor.Init));
        }

        public virtual void SetState(string state)
        {
            var request = new SetStateParameters();
            request.State = state;
            Channel.Send(request, Descriptor.SetState, GetCancellationToken(Descriptor.SetState));
        }

        public virtual string GetState()
        {
            return Channel.Send<string, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.GetState, GetCancellationToken(Descriptor.GetState));
        }

        public virtual Task<string> GetStateAsync()
        {
            return Channel.SendAsync<string, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.GetState, GetCancellationToken(Descriptor.GetState));
        }

        public virtual void Destroy()
        {
            Channel.Send(Bolt.Empty.Instance, Descriptor.Destroy, GetCancellationToken(Descriptor.Destroy));
        }
    }
}


