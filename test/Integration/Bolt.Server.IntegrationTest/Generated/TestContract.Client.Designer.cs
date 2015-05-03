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
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Server.IntegrationTest.Core.Parameters;


namespace Bolt.Server.IntegrationTest.Core
{
    public partial interface ITestContractInnerAsync : ITestContractInner
    {
        Task SimpleMethodWithComplexParameterAsync(Bolt.Test.Common.CompositeType compositeType);

        Task MethodWithNotSerializableTypeAsync(Bolt.Test.Common.NotSerializableType arg);

        Task<Bolt.Test.Common.NotSerializableType> FunctionWithNotSerializableTypeAsync();
    }
}

namespace Bolt.Server.IntegrationTest.Core
{
    public partial interface ITestContractAsync : ITestContract, ITestContractInnerAsync
    {
        Task SimpleMethodAsync();
    }
}

namespace Bolt.Server.IntegrationTest.Core
{
    public partial class TestContractProxy : ContractProxy<Bolt.Server.IntegrationTest.Core.TestContractDescriptor>, Bolt.Server.IntegrationTest.Core.ITestContract, ITestContractInnerAsync, ITestContractAsync
    {
        // useless comment added by user generator - 'Bolt.Server.IntegrationTest.Core.UserCodeGenerator', Context - 'generatorContext2'

        public TestContractProxy(Bolt.Server.IntegrationTest.Core.TestContractProxy proxy) : base(proxy)
        {
        }

        public TestContractProxy(IChannel channel) : base(channel)
        {
        }

        public virtual void SimpleMethodWithSimpleArguments(int val)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.SimpleMethodWithSimpleArgumentsParameters();
            bolt_Params.Val = val;
            Send(bolt_Params, Descriptor.SimpleMethodWithSimpleArguments, CancellationToken.None);
        }

        public virtual void SimpleMethod()
        {
            Send(Bolt.Empty.Instance, Descriptor.SimpleMethod, CancellationToken.None);
        }

        public virtual Task SimpleMethodAsync()
        {
            return SendAsync(Bolt.Empty.Instance, Descriptor.SimpleMethod, CancellationToken.None);
        }

        public virtual Task SimpleMethodExAsync()
        {
            return SendAsync(Bolt.Empty.Instance, Descriptor.SimpleMethodExAsync, CancellationToken.None);
        }

        public virtual void SimpleMethodWithCancellation(CancellationToken cancellation)
        {
            Send(Bolt.Empty.Instance, Descriptor.SimpleMethodWithCancellation, cancellation);
        }

        public virtual Bolt.Test.Common.CompositeType ComplexFunction()
        {
            return Send<Bolt.Test.Common.CompositeType, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.ComplexFunction, CancellationToken.None);
        }
        public virtual void SimpleMethodWithComplexParameter(Bolt.Test.Common.CompositeType compositeType)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.SimpleMethodWithComplexParameterParameters();
            bolt_Params.CompositeType = compositeType;
            Send(bolt_Params, Descriptor.SimpleMethodWithComplexParameter, CancellationToken.None);
        }

        public virtual Task SimpleMethodWithComplexParameterAsync(Bolt.Test.Common.CompositeType compositeType)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.SimpleMethodWithComplexParameterParameters();
            bolt_Params.CompositeType = compositeType;
            return SendAsync(bolt_Params, Descriptor.SimpleMethodWithComplexParameter, CancellationToken.None);
        }

        public virtual int SimpleFunction()
        {
            return Send<int, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.SimpleFunction, CancellationToken.None);
        }

        public virtual List<Bolt.Test.Common.CompositeType> FunctionReturningHugeData()
        {
            return Send<List<Bolt.Test.Common.CompositeType>, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.FunctionReturningHugeData, CancellationToken.None);
        }

        public virtual void MethodTakingHugeData(List<Bolt.Test.Common.CompositeType> arg)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.MethodTakingHugeDataParameters();
            bolt_Params.Arg = arg;
            Send(bolt_Params, Descriptor.MethodTakingHugeData, CancellationToken.None);
        }

        public virtual void MethodWithNotSerializableType(Bolt.Test.Common.NotSerializableType arg)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.MethodWithNotSerializableTypeParameters();
            bolt_Params.Arg = arg;
            Send(bolt_Params, Descriptor.MethodWithNotSerializableType, CancellationToken.None);
        }

        public virtual Task MethodWithNotSerializableTypeAsync(Bolt.Test.Common.NotSerializableType arg)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.MethodWithNotSerializableTypeParameters();
            bolt_Params.Arg = arg;
            return SendAsync(bolt_Params, Descriptor.MethodWithNotSerializableType, CancellationToken.None);
        }

        public virtual Bolt.Test.Common.NotSerializableType FunctionWithNotSerializableType()
        {
            return Send<Bolt.Test.Common.NotSerializableType, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.FunctionWithNotSerializableType, CancellationToken.None);
        }

        public virtual Task<Bolt.Test.Common.NotSerializableType> FunctionWithNotSerializableTypeAsync()
        {
            return SendAsync<Bolt.Test.Common.NotSerializableType, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.FunctionWithNotSerializableType, CancellationToken.None);
        }

        public virtual Task<int> SimpleAsyncFunction()
        {
            return SendAsync<int, Bolt.Empty>(Bolt.Empty.Instance, Descriptor.SimpleAsyncFunction, CancellationToken.None);
        }

        public virtual void MethodWithManyArguments(Bolt.Test.Common.CompositeType arg1, Bolt.Test.Common.CompositeType arg2, DateTime time)
        {
            var bolt_Params = new Bolt.Server.IntegrationTest.Core.Parameters.MethodWithManyArgumentsParameters();
            bolt_Params.Arg1 = arg1;
            bolt_Params.Arg2 = arg2;
            bolt_Params.Time = time;
            Send(bolt_Params, Descriptor.MethodWithManyArguments, CancellationToken.None);
        }
        public virtual void ThisMethodShouldBeExcluded()
        {
            Send(Bolt.Empty.Instance, Descriptor.ThisMethodShouldBeExcluded, CancellationToken.None);
        }
    }
}