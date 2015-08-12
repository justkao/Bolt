using System;
using System.Reflection;
using System.Threading.Tasks;

using Bolt.Common;
using Bolt.Pipeline;

using Castle.DynamicProxy;

namespace Bolt.Client.Proxy
{
    public class DynamicProxyFactory : ProxyFactory
    {
        public static readonly DynamicProxyFactory Default = new DynamicProxyFactory();

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        public override T CreateProxy<T>(IPipeline<ClientActionContext> pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            Type contract = typeof (T);

            if (!contract.IsInterface)
            {
                return base.CreateProxy<T>(pipeline);
            }

            BoltFramework.ValidateContract(contract);
            var interceptor = new ChannelInterceptor();
            var options = new ProxyGenerationOptions {
                BaseTypeForInterfaceProxy = typeof (DynamicContractProxy)
            };

            T proxy = _generator.CreateInterfaceProxyWithoutTarget<T>(options, interceptor);
            ((DynamicContractProxy) (object) proxy).Initialize(typeof (T), pipeline);
            interceptor.Proxy = ((DynamicContractProxy) (object) proxy);
            return proxy;
        }

        public class DynamicContractProxy : ProxyBase
        {
            internal void Initialize(Type contract, IPipeline<ClientActionContext> pipeline)
            {
                Pipeline = pipeline;
                Contract = contract;
            }
        }

        private class ChannelInterceptor : IInterceptor
        {
            public IProxy Proxy { get; set; }

            public void Intercept(IInvocation invocation)
            {
                if (typeof (Task).IsAssignableFrom(invocation.Method.ReturnType))
                {
                    Type innerType = TypeHelper.GetTaskInnerTypeOrNull(invocation.Method.ReturnType);
                    if (innerType == null)
                    {
                        // async method
                        invocation.ReturnValue = Proxy.SendAsync(invocation.Method, invocation.Arguments);
                    }
                    else
                    {
                        Task<object> result = Proxy.SendAsync(invocation.Method, invocation.Arguments);
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
                                invocation.ReturnValue = method.Invoke(null, new[] {(object) result});
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
                    invocation.ReturnValue = Proxy.Send(invocation.Method, invocation.Arguments);
                }
            }

            private static async Task<T> Convert<T>(Task<object> task)
            {
                object result = await task;
                return (T) result;
            }
        }
    }
}
