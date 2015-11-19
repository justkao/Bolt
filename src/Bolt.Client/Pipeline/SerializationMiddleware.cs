using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Metadata;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ClientActionContext>
    {
        public SerializationMiddleware(ISerializer serializer, IExceptionWrapper exceptionWrapper,
            IClientErrorProvider errorProvider)
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

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Serializer.MediaType));
            if (context.EnsureRequest().Content == null)
            {
                context.EnsureRequest().Content = await BuildRequestParametersAsync(context);
            }

            await Next(context);

            if (context.Response == null)
            {
                throw new BoltClientException(
                    $"Unable to process result for action '{context.Action.Name}' because response from server '{context.Request.RequestUri}' was not received.",
                    ClientErrorCode.DeserializeResponse,
                    context.Action);
            }

            TryHandleBoltServerError(context);

            await HandleResponseAsync(context);
        }

        protected virtual async Task HandleResponseAsync(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                Exception errorOnServer;
                using (Stream stream = await GetResponseStreamAsync(context.Response))
                {
                    errorOnServer = await DeserializeExceptionAsync(context, stream);
                }

                if (errorOnServer != null)
                {
                    context.ErrorResult = errorOnServer;
                    throw errorOnServer;
                }

                context.Response.EnsureSuccessStatusCode();
            }
            else
            {
                if (context.EnsureActionMetadata().HasResult && context.ActionResult == null)
                {
                    using (Stream stream = await GetResponseStreamAsync(context.Response))
                    {
                        context.ActionResult = await DeserializeResponseAsync(context, stream);
                    }
                }
            }
        }

        protected virtual async Task<Stream> GetResponseStreamAsync(HttpResponseMessage response)
        {
            return new MemoryStream(await response.Content.ReadAsByteArrayAsync());
        }

        protected virtual async Task<HttpContent> BuildRequestParametersAsync(ClientActionContext context)
        {
            ActionMetadata metadata = context.EnsureActionMetadata();
            if (metadata.HasSerializableParameters)
            {
                try
                {
                    metadata.ValidateParameters(context.Parameters);
                }
                catch (Exception e)
                {
                    throw new BoltClientException(
                        $"Parameter validation failed for action '{context.Action.Name}'.",
                        ClientErrorCode.SerializeParameters,
                        context.Action,
                        e);
                }
            }

            if (context.SerializeParametersContext == null)
            {
                context.SerializeParametersContext = CreateSerializeContext(context);
            }

            try
            {
                await Serializer.WriteAsync(context.SerializeParametersContext);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to serialize parameters for action '{context.Action.Name}'.",
                    ClientErrorCode.SerializeParameters,
                    context.Action,
                    e);
            }

            if (context.SerializeParametersContext.Stream.Length == 0)
            {
                return null;
            }

            context.SerializeParametersContext.Stream.Seek(0, SeekOrigin.Begin);
            StreamContent content = new StreamContent(context.SerializeParametersContext.Stream);
            content.Headers.ContentLength = context.SerializeParametersContext.Stream.Length;

            return content; 
        }

        private SerializeContext CreateSerializeContext(ClientActionContext context)
        {
            SerializeContext ctxt = new SerializeContext() { Stream = new MemoryStream() };
            ctxt.Values = new List<KeyValuePair<string, object>>();

            for (int i = 0; i < context.ActionMetadata.Parameters.Length; i++)
            {
                if (context.Parameters[i] == null)
                {
                    continue;
                }

                if (context.Parameters[i] is CancellationToken)
                {
                    continue;
                }

                ctxt.Values.Add(new KeyValuePair<string, object>(context.ActionMetadata.Parameters[i].Name, context.Parameters[i]));
            }

            return ctxt;
        }

        protected virtual async Task<object> DeserializeResponseAsync(ClientActionContext context, Stream stream)
        {
            if (stream.Length == 0)
            {
                return null;
            }

            try
            {
                return await Serializer.ReadAsync(context.EnsureActionMetadata().ResultType, stream);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to deserialize response for action '{context.Action.Name}'.",
                    ClientErrorCode.DeserializeResponse,
                    context.Action,
                    e);
            }
        }

        protected virtual async Task<Exception> DeserializeExceptionAsync(ClientActionContext context, Stream stream)
        {
            if (stream.Length == 0)
            {
                return null;
            }

            try
            {
                object result = await Serializer.ReadAsync(ExceptionWrapper.Type, stream);
                if (result == null)
                {
                    return null;
                }

                return ExceptionWrapper.Unwrap(result);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to deserialize exception response for action '{context.Action.Name}'.",
                    ClientErrorCode.DeserializeExceptionResponse,
                    context.Action,
                    e);
            }
        }

        protected virtual void TryHandleBoltServerError(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                BoltServerException boltError = ErrorProvider.TryReadServerError(context);
                if (boltError != null)
                {
                    throw boltError;
                }
            }
        }
    }
}
