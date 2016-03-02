using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Metadata;
using Castle.DynamicProxy;

namespace Bolt.Client.Proxy
{
    public class DynamicProxyFactory : ProxyFactory
    {
        public static readonly DynamicProxyFactory Default = new DynamicProxyFactory();

        private readonly ProxyGenerator _generator = new ProxyGenerator();
        private readonly ConcurrentDictionary<Type, ProxyMetadata> _metadatas= new ConcurrentDictionary<Type, ProxyMetadata>(); 

        public override T CreateProxy<T>(IClientPipeline pipeline)
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
            var options = new ProxyGenerationOptions
            {
                BaseTypeForInterfaceProxy = typeof (DynamicContractProxy)
            };

            ProxyMetadata metadata = _metadatas.GetOrAdd(typeof (T), v => new ProxyMetadata());
            T proxy = _generator.CreateInterfaceProxyWithoutTarget<T>(options, interceptor);
            ((DynamicContractProxy) (object) proxy).Initialize(metadata, typeof(T), pipeline);
            interceptor.Proxy = (DynamicContractProxy) (object) proxy;
            return proxy;
        }

        public class DynamicContractProxy : ProxyBase
        {
            internal ProxyMetadata Metadata { get; private set; }

            internal void Initialize(ProxyMetadata metadata, Type contract, IClientPipeline pipeline)
            {
                Pipeline = pipeline;
                Contract = contract;
                Metadata = metadata;
            }
        }

        private class ChannelInterceptor : IInterceptor
        {
            public DynamicContractProxy Proxy { get; set; }

            public void Intercept(IInvocation invocation)
            {
                MethodMetadata metadata = Proxy.Metadata.Get(invocation.Method);
                if (metadata.ActionMetadata.IsAsync)
                {
                    if (!metadata.ActionMetadata.HasResult)
                    {
                        // async method
                        invocation.ReturnValue = Proxy.SendAsync(invocation.Method, invocation.Arguments);
                    }
                    else
                    {
                        Task<object> result = Proxy.SendAsync(invocation.Method, invocation.Arguments);

                        if (metadata.ActionMetadata.ResultType == typeof (object))
                        {
                            invocation.ReturnValue = result;
                        }
                        else
                        {
                            invocation.ReturnValue = metadata.AsyncValueProvider(result);
                        }
                    }
                }
                else
                {
                    // not async method
                    invocation.ReturnValue = Proxy.Send(invocation.Method, invocation.Arguments);
                }
            }
        }

        internal class ProxyMetadata : ValueCache<MethodInfo, MethodMetadata>
        {
            public MethodMetadata Get(MethodInfo info)
            {
                return base.Get(info);
            }

            protected override MethodMetadata Create(MethodInfo key, object context)
            {
                ActionMetadata metadata = BoltFramework.ActionMetadata.Resolve(key);
                Func<Task<object>, Task> provider = null;
                if (metadata.IsAsync && metadata.HasResult)
                {
                    provider = MethodInvokerBuilder.Build(metadata.ResultType);
                }

                return new MethodMetadata(metadata, provider);
            }
        }

        internal class MethodMetadata
        {
            public MethodMetadata(ActionMetadata actionMetadata, Func<Task<object>, Task> asyncValueProvider)
            {
                ActionMetadata = actionMetadata;
                AsyncValueProvider = asyncValueProvider;
            }

            public ActionMetadata ActionMetadata { get; }

            public Func<Task<object>, Task> AsyncValueProvider { get; }
        }

        private static class MethodInvokerBuilder
        {
            public static Func<Task<object>, Task> Build(Type resultType)
            {
                // lambda parameters
                ParameterExpression taskParam = Expression.Parameter(typeof(Task<object>), "task");

                MethodInfo convertTaskMethod = typeof(MethodInvokerBuilder).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(ConvertTask));
                convertTaskMethod = convertTaskMethod.MakeGenericMethod(resultType);

                // compile lambda
                return Expression.Lambda<Func<Task<object>, Task>>(Expression.Call(convertTaskMethod, taskParam), taskParam).Compile();
            }

            private static Task<T> ConvertTask<T>(Task<object> task)
            {
                return task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        // this will cause the exception to be thrown
                        t.GetAwaiter().GetResult();
                    }

                    return (T) t.Result;
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
    }
}
