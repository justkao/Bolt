
using Bolt;
using Bolt.Server;
using Bolt.Service.Test.Core;
using Bolt.Service.Test.Core.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bolt.Service.Test.Core.Parameters
{
    [DataContract]
    public partial class SimpleMethodWithSimpleArgumentsParameters
    {
        [DataMember(Order = 1)]
        public int Val { get; set; }
    }

    [DataContract]
    public partial class SimpleMethodWithCancellationParameters
    {
    }

    [DataContract]
    public partial class SimpleMethodWithComplexParameterParameters
    {
        [DataMember(Order = 1)]
        public CompositeType CompositeType { get; set; }
    }

}

namespace Bolt.Service.Test.Core
{
    public partial class TestContractDescriptor : ContractDescriptor
    {
        public TestContractDescriptor() : base(typeof(Bolt.Service.Test.Core.ITestContract))
        {
            SimpleMethodWithSimpleArguments = Add("SimpleMethodWithSimpleArguments", typeof(Bolt.Service.Test.Core.Parameters.SimpleMethodWithSimpleArgumentsParameters), typeof(ITestContract).GetTypeInfo().GetMethod("SimpleMethodWithSimpleArguments"));
            SimpleMethod = Add("SimpleMethod", typeof(Bolt.Empty), typeof(ITestContract).GetTypeInfo().GetMethod("SimpleMethod"));
            SimpleMethodExAsync = Add("SimpleMethodExAsync", typeof(Bolt.Empty), typeof(ITestContract).GetTypeInfo().GetMethod("SimpleMethodExAsync"));
            SimpleMethodWithCancellation = Add("SimpleMethodWithCancellation", typeof(Bolt.Service.Test.Core.Parameters.SimpleMethodWithCancellationParameters), typeof(ITestContract).GetTypeInfo().GetMethod("SimpleMethodWithCancellation"));
            ComplexFunction = Add("ComplexFunction", typeof(Bolt.Empty), typeof(ITestContract).GetTypeInfo().GetMethod("ComplexFunction"));
            SimpleMethodWithComplexParameter = Add("SimpleMethodWithComplexParameter", typeof(Bolt.Service.Test.Core.Parameters.SimpleMethodWithComplexParameterParameters), typeof(ITestContractInner).GetTypeInfo().GetMethod("SimpleMethodWithComplexParameter"));
            SimpleFunction = Add("SimpleFunction", typeof(Bolt.Empty), typeof(ITestContractInner).GetTypeInfo().GetMethod("SimpleFunction"));
        }

        public static readonly TestContractDescriptor Default = new TestContractDescriptor();

        public virtual ActionDescriptor SimpleMethodWithSimpleArguments { get; set; }

        public virtual ActionDescriptor SimpleMethod { get; set; }

        public virtual ActionDescriptor SimpleMethodExAsync { get; set; }

        public virtual ActionDescriptor SimpleMethodWithCancellation { get; set; }

        public virtual ActionDescriptor ComplexFunction { get; set; }

        public virtual ActionDescriptor SimpleMethodWithComplexParameter { get; set; }

        public virtual ActionDescriptor SimpleFunction { get; set; }
    }
}

namespace Bolt.Service.Test.Core
{
    public interface ITestContractInnerAsync : ITestContractInner
    {
        Task SimpleMethodWithComplexParameterAsync(CompositeType compositeType);
    }
}

namespace Bolt.Service.Test.Core
{
    public interface ITestContractAsync : ITestContract, ITestContractInnerAsync
    {
        Task SimpleMethodAsync();
    }
}

namespace Bolt.Service.Test.Core
{
    public partial class TestContractChannel : Bolt.Client.Channel, Bolt.Service.Test.Core.ITestContract
    {
        public Bolt.Service.Test.Core.TestContractDescriptor ContractDescriptor { get; set; }

        public virtual void SimpleMethodWithSimpleArguments(int val)
        {
            var request = new SimpleMethodWithSimpleArgumentsParameters();
            request.Val = val;
            var descriptor = ContractDescriptor.SimpleMethodWithSimpleArguments;
            var token = GetCancellationToken(descriptor);

            Send(request, descriptor, token);
        }

        public virtual void SimpleMethod()
        {
            var descriptor = ContractDescriptor.SimpleMethod;
            var token = GetCancellationToken(descriptor);

            Send(Empty.Instance, descriptor, token);
        }

        public virtual Task SimpleMethodAsync()
        {
            var descriptor = ContractDescriptor.SimpleMethod;
            var token = GetCancellationToken(descriptor);

            return SendAsync(Empty.Instance, descriptor, token);
        }

        public virtual Task SimpleMethodExAsync()
        {
            var descriptor = ContractDescriptor.SimpleMethodExAsync;
            var token = GetCancellationToken(descriptor);

            return SendAsync(Empty.Instance, descriptor, token);
        }

        public virtual void SimpleMethodWithCancellation(System.Threading.CancellationToken cancellation)
        {
            var request = new SimpleMethodWithCancellationParameters();
            var descriptor = ContractDescriptor.SimpleMethodWithCancellation;
            Send(request, descriptor, cancellation);
        }

        public virtual CompositeType ComplexFunction()
        {
            var descriptor = ContractDescriptor.ComplexFunction;
            var token = GetCancellationToken(descriptor);

            return Send<CompositeType, Empty>(Empty.Instance, descriptor, token);
        }
        public virtual void SimpleMethodWithComplexParameter(CompositeType compositeType)
        {
            var request = new SimpleMethodWithComplexParameterParameters();
            request.CompositeType = compositeType;
            var descriptor = ContractDescriptor.SimpleMethodWithComplexParameter;
            var token = GetCancellationToken(descriptor);

            Send(request, descriptor, token);
        }

        public virtual Task SimpleMethodWithComplexParameterAsync(CompositeType compositeType)
        {
            var request = new SimpleMethodWithComplexParameterParameters();
            request.CompositeType = compositeType;
            var descriptor = ContractDescriptor.SimpleMethodWithComplexParameter;
            var token = GetCancellationToken(descriptor);

            return SendAsync(request, descriptor, token);
        }

        public virtual int SimpleFunction()
        {
            var descriptor = ContractDescriptor.SimpleFunction;
            var token = GetCancellationToken(descriptor);

            return Send<int, Empty>(Empty.Instance, descriptor, token);
        }
    }
}

namespace Bolt.Server
{
    public partial class TestContractExecutor : Bolt.Server.Executor
    {
        public override void Init()
        {
            if (ContractDescriptor == null)
            {
                ContractDescriptor = Bolt.Service.Test.Core.TestContractDescriptor.Default;
            }

            AddAction(ContractDescriptor.SimpleMethodWithSimpleArguments, TestContract_SimpleMethodWithSimpleArguments);
            AddAction(ContractDescriptor.SimpleMethod, TestContract_SimpleMethod);
            AddAction(ContractDescriptor.SimpleMethodExAsync, TestContract_SimpleMethodExAsync);
            AddAction(ContractDescriptor.SimpleMethodWithCancellation, TestContract_SimpleMethodWithCancellation);
            AddAction(ContractDescriptor.ComplexFunction, TestContract_ComplexFunction);
            AddAction(ContractDescriptor.SimpleMethodWithComplexParameter, TestContractInner_SimpleMethodWithComplexParameter);
            AddAction(ContractDescriptor.SimpleFunction, TestContractInner_SimpleFunction);

            base.Init();
        }

        public virtual Bolt.Service.Test.Core.TestContractDescriptor ContractDescriptor { get; set; }

        protected virtual async Task TestContract_SimpleMethodWithSimpleArguments(Bolt.Server.ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<SimpleMethodWithSimpleArgumentsParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<ITestContract>(context);
            instance.SimpleMethodWithSimpleArguments(parameters.Val);
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContract_SimpleMethod(Bolt.Server.ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<ITestContract>(context);
            instance.SimpleMethod();
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContract_SimpleMethodExAsync(Bolt.Server.ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<ITestContract>(context);
            await instance.SimpleMethodExAsync();
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContract_SimpleMethodWithCancellation(Bolt.Server.ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<SimpleMethodWithCancellationParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<ITestContract>(context);
            instance.SimpleMethodWithCancellation(context.CallCancelled);
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContract_ComplexFunction(Bolt.Server.ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<ITestContract>(context);
            var result = instance.ComplexFunction();
            await ResponseHandler.Handle(context, result);
        }

        protected virtual async Task TestContractInner_SimpleMethodWithComplexParameter(Bolt.Server.ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<SimpleMethodWithComplexParameterParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<ITestContractInner>(context);
            instance.SimpleMethodWithComplexParameter(parameters.CompositeType);
            await ResponseHandler.Handle(context);
        }

        protected virtual async Task TestContractInner_SimpleFunction(Bolt.Server.ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<ITestContractInner>(context);
            var result = instance.SimpleFunction();
            await ResponseHandler.Handle(context, result);
        }
    }
}

