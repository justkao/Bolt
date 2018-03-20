using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Metadata;
using Castle.DynamicProxy;

namespace Bolt.Client
{
    public sealed class ProxyFactory : IProxyFactory
    {
        public static readonly ProxyFactory Default = new ProxyFactory();

        private readonly ProxyGenerator _generator = new ProxyGenerator();
        private readonly ConcurrentDictionary<Type, ProxyMetadata> _metadatas = new ConcurrentDictionary<Type, ProxyMetadata>();
        private readonly Type _baseProxy;

        public ProxyFactory() : this(typeof(ProxyBase))
        {
        }

        public ProxyFactory(Type baseProxy)
        {
            if (baseProxy == null)
            {
                throw new ArgumentNullException(nameof(baseProxy));
            }

            if (!typeof(ProxyBase).GetTypeInfo().IsAssignableFrom(baseProxy.GetTypeInfo()))
            {
                throw new ArgumentException(
                    $"Invalid base proxy type. The base class must derive from {typeof(ProxyBase).FullName}.");
            }

            _baseProxy = baseProxy;
        }

        public T CreateProxy<T>(IClientPipeline pipeline) where T : class
        {
            ContractMetadata contract = BoltFramework.GetContract(typeof(T));
            var interceptor = new ChannelInterceptor();
            var options = new ProxyGenerationOptions
            {
                BaseTypeForInterfaceProxy = _baseProxy
            };

            ProxyMetadata metadata = _metadatas.GetOrAdd(typeof(T), v => new ProxyMetadata(_baseProxy));
            var proxy = _generator.CreateInterfaceProxyWithoutTarget<T>(
                options,
                interceptor);

            ProxyBase proxyBase = (ProxyBase)(object)proxy;
            proxyBase.Contract = contract;
            proxyBase.Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

            interceptor.Proxy = proxyBase;
            interceptor.Metadata = metadata;
            return proxy;
        }

        internal class ProxyMetadata : ValueCache<MethodInfo, MethodMetadata>
        {
            private readonly Type _proxyBase;

            public ProxyMetadata(Type proxyBase)
            {
                _proxyBase = proxyBase;
            }

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

                MethodInfo method = _proxyBase.GetRuntimeMethod(key.Name, key.GetParameters().Select(p => p.ParameterType).ToArray());
                return new MethodMetadata(metadata, provider, method);
            }
        }

        internal class MethodMetadata
        {
            public MethodMetadata(ActionMetadata actionMetadata, Func<Task<object>, Task> asyncValueProvider, MethodInfo invocationTarget)
            {
                ActionMetadata = actionMetadata;
                AsyncValueProvider = asyncValueProvider;
                InvocationTarget = invocationTarget;
            }

            public ActionMetadata ActionMetadata { get; }

            public MethodInfo InvocationTarget { get; }

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
                return task.ContinueWith(
                t =>
                {
                    if (t.Exception != null)
                    {
                        // this will cause the exception to be thrown
                        t.GetAwaiter().GetResult();
                    }

                    return (T)t.Result;
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private class ChannelInterceptor : IInterceptor
        {
            public ProxyBase Proxy { get; set; }

            public ProxyMetadata Metadata { get; set; }

            public void Intercept(IInvocation invocation)
            {
                MethodMetadata metadata = Metadata.Get(invocation.Method);

                if (metadata.InvocationTarget != null)
                {
                    try
                    {
                        // currently invoked using reflection, if required we might switch to invocation using expressions
                        invocation.ReturnValue = metadata.InvocationTarget.Invoke(invocation.Proxy, invocation.Arguments);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw e.InnerException;
                    }

                    return;
                }

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

                        if (metadata.ActionMetadata.ResultType == typeof(object))
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
    }
}
