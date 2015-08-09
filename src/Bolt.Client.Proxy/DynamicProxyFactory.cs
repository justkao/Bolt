using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Bolt.Common;
using Bolt.Core;
using Castle.DynamicProxy;

namespace Bolt.Client.Proxy
{
    public class DynamicProxyFactory : ProxyFactory
    {
        public static readonly DynamicProxyFactory Default = new DynamicProxyFactory();

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        public override T CreateProxy<T>(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            Type contract = typeof (T);

            if (!contract.IsInterface)
            {
                return base.CreateProxy<T>(channel);
            }

            BoltFramework.ValidateContract(contract);
            var interceptor = new ChannelInterceptor(contract, channel);
            var options = new ProxyGenerationOptions()
            {
                BaseTypeForInterfaceProxy = typeof (DynamicContractProxy),
            };

            T proxy = _generator.CreateInterfaceProxyWithoutTarget<T>(options, interceptor);
            ((DynamicContractProxy) (object) proxy).Initialize(typeof (T), channel);
            return proxy;
        }

        public class DynamicContractProxy : ContractProxy
        {
            internal void Initialize(Type contract, IChannel channel)
            {
                Channel = channel;
                Contract = contract;
            }
        }

        private class ChannelInterceptor : IInterceptor
        {
            private readonly Type _contract;

            private readonly IChannel _channel;

            public ChannelInterceptor(Type contract, IChannel channel)
            {
                _contract = contract;
                _channel = channel;
            }

            public void Intercept(IInvocation invocation)
            {
                CancellationToken cancellation;
                IObjectSerializer parameters = BuildParameters(invocation, out cancellation);

                if (typeof (Task).IsAssignableFrom(invocation.Method.ReturnType))
                {
                    Type innerType = TypeHelper.GetTaskInnerTypeOrNull(invocation.Method.ReturnType);
                    if (innerType == null)
                    {
                        // async method
                        invocation.ReturnValue = _channel.SendAsync(_contract, invocation.Method, typeof (void),
                            parameters, cancellation);
                    }
                    else
                    {
                        Task<object> result = _channel.SendAsync(_contract, invocation.Method, innerType, parameters,
                            cancellation);
                        if (innerType == typeof (object))
                        {
                            invocation.ReturnValue = result;
                        }
                        else
                        {
                            MethodInfo method =
                                typeof (ChannelInterceptor).GetMethod(nameof(Convert),
                                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                    .MakeGenericMethod(innerType);
                            try
                            {
                                invocation.ReturnValue = method.Invoke(null, new[] { (object)result });
                            }
                            catch (TargetInvocationException e)
                            {
                                throw e.InnerException;
                            }
                        }
                    }
                }
                else
                {
                    // not async method
                    invocation.ReturnValue = _channel.Send(_contract, invocation.Method, invocation.Method.ReturnType,
                        parameters, cancellation);
                }
            }

            private IObjectSerializer BuildParameters(IInvocation invocation, out CancellationToken cancellation)
            {
                cancellation = CancellationToken.None;
                IObjectSerializer parameterSerializer = null;
                ParameterInfo[] parameters = invocation.Method.GetParameters();

                for (int i = 0; i < invocation.Arguments.Length; i++)
                {
                    if (invocation.Arguments[i] is CancellationToken)
                    {
                        cancellation = (CancellationToken) invocation.Arguments[i];
                        continue;
                    }

                    if (parameterSerializer == null)
                    {
                        parameterSerializer = _channel.Serializer.CreateSerializer();
                    }

                    parameterSerializer.WriteParameter(invocation.Method, parameters[i].Name,
                        parameters[i].ParameterType,
                        invocation.GetArgumentValue(i));
                }

                return parameterSerializer;
            }

            private static async Task<T> Convert<T>(Task<object> task)
            {
                object result = await task;
                return (T) result;
            }
        }
    }
}
