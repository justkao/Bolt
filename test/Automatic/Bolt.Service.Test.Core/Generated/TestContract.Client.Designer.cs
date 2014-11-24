//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.

//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Bolt.Generators;
using Bolt.Service.Test.Core;
using Bolt.Service.Test.Core.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bolt.Service.Test.Core
{
    public interface ITestContractInnerAsync : ITestContractInner
    {
        Task SimpleMethodWithComplexParameterAsync(CompositeType compositeType);

        Task<int> SimpleFunctionAsync();

        Task MethodWithManyArgumentsAsync(CompositeType arg1, CompositeType arg2, DateTime time);
    }
}

namespace Bolt.Service.Test.Core
{
    public interface ITestContractAsync : ITestContract, ITestContractInnerAsync
    {
        Task SimpleMethodWithSimpleArgumentsAsync(int val);

        Task SimpleMethodAsync();

        Task SimpleMethodWithCancellationAsync(System.Threading.CancellationToken cancellation);

        Task<CompositeType> ComplexFunctionAsync();
    }
}

namespace Bolt.Service.Test.Core
{
    public partial class TestContractChannel : Bolt.Client.Channel, Bolt.Service.Test.Core.ITestContract, Bolt.Client.IContractDescriptorProvider<TestContractDescriptor>
    {
        public Bolt.Service.Test.Core.TestContractDescriptor TestContractDescriptor { get; set; }

        TestContractDescriptor Bolt.Client.IContractDescriptorProvider<TestContractDescriptor>.Descriptor
        {
            get
            {
                return this.TestContractDescriptor;
            }
            set
            {
                this.TestContractDescriptor = value;
            }
        }

        public virtual void SimpleMethodWithSimpleArguments(int val)
        {
            var request = new SimpleMethodWithSimpleArgumentsParameters();
            request.Val = val;
            Send(request, TestContractDescriptor.SimpleMethodWithSimpleArguments, GetCancellationToken(TestContractDescriptor.SimpleMethodWithSimpleArguments));
        }

        public virtual Task SimpleMethodWithSimpleArgumentsAsync(int val)
        {
            var request = new SimpleMethodWithSimpleArgumentsParameters();
            request.Val = val;
            return SendAsync(request, TestContractDescriptor.SimpleMethodWithSimpleArguments, GetCancellationToken(TestContractDescriptor.SimpleMethodWithSimpleArguments));
        }

        public virtual void SimpleMethod()
        {
            Send(Bolt.Empty.Instance, TestContractDescriptor.SimpleMethod, GetCancellationToken(TestContractDescriptor.SimpleMethod));
        }

        public virtual Task SimpleMethodAsync()
        {
            return SendAsync(Bolt.Empty.Instance, TestContractDescriptor.SimpleMethod, GetCancellationToken(TestContractDescriptor.SimpleMethod));
        }

        public virtual Task SimpleMethodExAsync()
        {
            return SendAsync(Bolt.Empty.Instance, TestContractDescriptor.SimpleMethodExAsync, GetCancellationToken(TestContractDescriptor.SimpleMethodExAsync));
        }

        public virtual void SimpleMethodWithCancellation(System.Threading.CancellationToken cancellation)
        {
            Send(Bolt.Empty.Instance, TestContractDescriptor.SimpleMethodWithCancellation, cancellation);
        }

        public virtual Task SimpleMethodWithCancellationAsync(System.Threading.CancellationToken cancellation)
        {
            return SendAsync(Bolt.Empty.Instance, TestContractDescriptor.SimpleMethodWithCancellation, cancellation);
        }

        public virtual CompositeType ComplexFunction()
        {
            return Send<CompositeType, Bolt.Empty>(Bolt.Empty.Instance, TestContractDescriptor.ComplexFunction, GetCancellationToken(TestContractDescriptor.ComplexFunction));
        }

        public virtual Task<CompositeType> ComplexFunctionAsync()
        {
            return SendAsync<CompositeType, Bolt.Empty>(Bolt.Empty.Instance, TestContractDescriptor.ComplexFunction, GetCancellationToken(TestContractDescriptor.ComplexFunction));
        }

        public virtual void SimpleMethodWithComplexParameter(CompositeType compositeType)
        {
            var request = new SimpleMethodWithComplexParameterParameters();
            request.CompositeType = compositeType;
            Send(request, TestContractDescriptor.SimpleMethodWithComplexParameter, GetCancellationToken(TestContractDescriptor.SimpleMethodWithComplexParameter));
        }

        public virtual Task SimpleMethodWithComplexParameterAsync(CompositeType compositeType)
        {
            var request = new SimpleMethodWithComplexParameterParameters();
            request.CompositeType = compositeType;
            return SendAsync(request, TestContractDescriptor.SimpleMethodWithComplexParameter, GetCancellationToken(TestContractDescriptor.SimpleMethodWithComplexParameter));
        }

        public virtual int SimpleFunction()
        {
            return Send<int, Bolt.Empty>(Bolt.Empty.Instance, TestContractDescriptor.SimpleFunction, GetCancellationToken(TestContractDescriptor.SimpleFunction));
        }

        public virtual Task<int> SimpleFunctionAsync()
        {
            return SendAsync<int, Bolt.Empty>(Bolt.Empty.Instance, TestContractDescriptor.SimpleFunction, GetCancellationToken(TestContractDescriptor.SimpleFunction));
        }

        public virtual Task<int> SimpleAsyncFunction()
        {
            return SendAsync<int, Bolt.Empty>(Bolt.Empty.Instance, TestContractDescriptor.SimpleAsyncFunction, GetCancellationToken(TestContractDescriptor.SimpleAsyncFunction));
        }

        public virtual void MethodWithManyArguments(CompositeType arg1, CompositeType arg2, DateTime time)
        {
            var request = new MethodWithManyArgumentsParameters();
            request.Arg1 = arg1;
            request.Arg2 = arg2;
            request.Time = time;
            Send(request, TestContractDescriptor.MethodWithManyArguments, GetCancellationToken(TestContractDescriptor.MethodWithManyArguments));
        }

        public virtual Task MethodWithManyArgumentsAsync(CompositeType arg1, CompositeType arg2, DateTime time)
        {
            var request = new MethodWithManyArgumentsParameters();
            request.Arg1 = arg1;
            request.Arg2 = arg2;
            request.Time = time;
            return SendAsync(request, TestContractDescriptor.MethodWithManyArguments, GetCancellationToken(TestContractDescriptor.MethodWithManyArguments));
        }

    }
}


namespace Bolt.Service.Test.Core
{
    public partial class TestContractChannelFactory : Bolt.Client.ChannelFactory<TestContractChannel, TestContractDescriptor>
    {
        public TestContractChannelFactory() : this(TestContractDescriptor.Default)
        {
        }

        public TestContractChannelFactory(TestContractDescriptor descriptor) : base(descriptor)
        {
        }
    }
}


