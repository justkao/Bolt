using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Core;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ClientActionContext>
    {
        public SerializationMiddleware( ISerializer serializer, IExceptionWrapper exceptionWrapper, IClientErrorProvider errorProvider)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (exceptionWrapper == null) throw new ArgumentNullException(nameof(exceptionWrapper));
            if (errorProvider == null) throw new ArgumentNullException(nameof(errorProvider));

            Serializer = serializer;
            ExceptionWrapper = exceptionWrapper;
            ErrorProvider = errorProvider;
        }

        public ISerializer Serializer { get; }

        public IExceptionWrapper ExceptionWrapper { get; }

        public IClientErrorProvider ErrorProvider { get; }

        public override async Task Invoke(ClientActionContext context)
        {
            if (context.Request.Content == null && context.HasSerializableParameters)
            {
                context.Request.Content = BuildRequestParameters(context);
            }

            await Next(context);

            if (context.Response == null)
            {
                throw new BoltException($"Unable to process result for action '{context.Action.Name}' because response from server '{context.Request.RequestUri}' was not received.");
            }

            await HandleResponseAsync(context);
        }

        protected virtual HttpContent BuildRequestParameters(ClientActionContext context)
        {
            IObjectSerializer parameterSerializer = Serializer.CreateSerializer();
            ParameterInfo[] parameters = context.Action.GetParameters();

            for (int i = 0; i < context.Parameters.Length; i++)
            {
                if (context.Parameters[i] == null)
                {
                    continue;
                }

                if (context.Parameters[i] is CancellationToken)
                {
                    continue;
                }

                parameterSerializer.WriteParameter(context.Action, parameters[i].Name, parameters[i].GetType(), parameters[i]);
            }

            if (parameterSerializer.IsEmpty)
            {
                return null;
            }

            Stream resultStream = parameterSerializer.GetOutputStream();
            byte[] rawData;
            if (resultStream is MemoryStream)
            {
                rawData = (resultStream as MemoryStream).ToArray();
            }
            else
            {
                rawData = resultStream.Copy().ToArray();
            }

            ByteArrayContent content = new ByteArrayContent(rawData);
            content.Headers.ContentLength = rawData.Length;
            context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Serializer.ContentType));
            return content;
        }

        protected virtual async Task<object> ReadResponseAsync(ClientActionContext context)
        {
            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return null;
                }

                return Serializer.DeserializeResponse(context.ResponseType, stream, context.Action);
            }
        }

        protected virtual async Task<Exception> ReadExceptionAsync(ClientActionContext context)
        {
            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return null;
                }

                object result = Serializer.DeserializeExceptionResponse(ExceptionWrapper.Type, stream, context.Action);
                if (result == null)
                {
                    return null;
                }

                return ExceptionWrapper.Unwrap(result);
            }
        }

        protected virtual async Task HandleResponseAsync(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                BoltServerException boltError = ReadBoltServerErrorIfAvailable(context);
                if (boltError != null)
                {
                    throw boltError;
                }

                Exception errorOnServer = await ReadExceptionAsync(context);
                if (errorOnServer != null)
                {
                    context.ErrorResult = errorOnServer;
                    throw errorOnServer;
                }

                context.Response.EnsureSuccessStatusCode();
            }
            else
            {
                if (context.HasSerializableActionResult)
                {
                    context.ActionResult = await ReadResponseAsync(context);
                }
            }
        }

        protected virtual BoltServerException ReadBoltServerErrorIfAvailable(ClientActionContext context)
        {
            return ErrorProvider.TryReadServerError(context);
        }
    }
}
