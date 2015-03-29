using System;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ServerDataHandler : IServerDataHandler
    {
        private readonly IExceptionWrapper _exceptionWrapper;
        private static readonly Task<Empty> EmptyParametersTask = Task.FromResult(Empty.Instance);

        public ServerDataHandler(ISerializer serializer, IExceptionWrapper exceptionWrapper, IParameterBinder parameterBinder)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (exceptionWrapper == null)
            {
                throw new ArgumentNullException(nameof(exceptionWrapper));
            }

            if (parameterBinder == null)
            {
                throw new ArgumentNullException(nameof(parameterBinder));
            }

            Serializer = serializer;
            ParameterBinder = parameterBinder;
            _exceptionWrapper = exceptionWrapper;
        }

        public virtual string ContentType => Serializer.ContentType;

        public ISerializer Serializer { get; }

        public IParameterBinder ParameterBinder { get; }

        public virtual async Task<T> ReadParametersAsync<T>(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();

            if (!context.Action.HasParameters)
            {
                return (T)(object)Empty.Instance;
            }

            if (context.Context.Request.Method == "GET")
            {
                return await ParameterBinder.BindParametersAsync<T>(context);
            }

            return Serializer.DeserializeParameters<T>(await context.Context.Request.Body.CopyAsync(context.RequestAborted), context.Action);
        }

        public virtual Task WriteResponseAsync<T>(ServerActionContext context, T data)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            byte[] raw = Serializer.SerializeResponse(data, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = Serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }

        public virtual Task WriteExceptionAsync(ServerActionContext context, Exception exception)
        {
            context.RequestAborted.ThrowIfCancellationRequested();

            var wrappedException = _exceptionWrapper.Wrap(exception);
            if (wrappedException == null)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            byte[] raw = Serializer.SerializeResponse(wrappedException, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = Serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }
    }
}