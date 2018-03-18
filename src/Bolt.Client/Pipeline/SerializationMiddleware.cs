using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Metadata;
using Bolt.Pipeline;
using Bolt.Serialization;

namespace Bolt.Client.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ClientActionContext>
    {
        private readonly MediaTypeWithQualityHeaderValue _acceptHeader;

        public SerializationMiddleware(ISerializer serializer, IExceptionSerializer exceptionSerializer, IClientErrorProvider errorProvider)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            ExceptionSerializer = exceptionSerializer ?? throw new ArgumentNullException(nameof(exceptionSerializer));
            ErrorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));

            _acceptHeader = new MediaTypeWithQualityHeaderValue(Serializer.MediaType);
        }

        public ISerializer Serializer { get; }

        public IExceptionSerializer ExceptionSerializer { get; }

        public IClientErrorProvider ErrorProvider { get; }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.Request.Headers.Accept.Add(_acceptHeader);
            if (context.GetRequestOrThrow().Content == null)
            {
                context.GetRequestOrThrow().Content = BuildRequestContent(context);
            }

            await Next(context).ConfigureAwait(false);

            if (context.Response == null)
            {
                throw new BoltClientException(
                    $"Unable to process result for action '{context.Action.Name}' because response from server '{context.Request.RequestUri}' was not received.",
                    ClientErrorCode.DeserializeResponse,
                    context.Action.Name);
            }

            TryHandleBoltServerError(context);

            await HandleResponseAsync(context).ConfigureAwait(false);
        }

        protected virtual async Task HandleResponseAsync(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                Exception errorOnServer;
                using (Stream stream = await GetResponseStreamAsync(context.Response).ConfigureAwait(false))
                {
                    errorOnServer = await DeserializeExceptionAsync(context, stream).ConfigureAwait(false);
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
                if (context.GetActionOrThrow().HasResult && context.ActionResult == null)
                {
                    using (Stream stream = await GetResponseStreamAsync(context.Response).ConfigureAwait(false))
                    {
                        context.ActionResult = await DeserializeResponseAsync(context, stream).ConfigureAwait(false);
                    }
                }
            }
        }

        protected virtual Task<Stream> GetResponseStreamAsync(HttpResponseMessage response)
        {
            return response.Content.ReadAsStreamAsync();
        }

        protected virtual HttpContent BuildRequestContent(ClientActionContext context)
        {
            ActionMetadata metadata = context.GetActionOrThrow();
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
                        context.Action.Name,
                        e);
                }
            }

            if (context.Action.HasSerializableParameters)
            {
                return new SerializeParametersContent(Serializer, context);
            }

            return null;
        }

        protected virtual async Task<object> DeserializeResponseAsync(ClientActionContext context, Stream stream)
        {
            try
            {
                return await Serializer.ReadAsync(stream, context.Action.ResultType).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to deserialize response for action '{context.Action.Name}'.",
                    ClientErrorCode.DeserializeResponse,
                    context.Action.Name,
                    e);
            }
        }

        protected virtual async Task<Exception> DeserializeExceptionAsync(ClientActionContext context, Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            try
            {
                var value = await Serializer.ReadAsync(stream, ExceptionSerializer.Type).ConfigureAwait(false);
                if (value == null)
                {
                    return null;
                }

                return ExceptionSerializer.Read(value);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to deserialize exception response for action '{context.Action.Name}'.",
                    ClientErrorCode.DeserializeExceptionResponse,
                    context.Action.Name,
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

        private class SerializeParametersContent : HttpContent
        {
            private readonly ClientActionContext _clientContext;
            private readonly ISerializer _serializer;

            public SerializeParametersContent(ISerializer serializer, ClientActionContext clientContext)
            {
                _serializer = serializer;
                _clientContext = clientContext;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                try
                {
                    await _serializer.WriteParametersAsync(stream, _clientContext.Action.Parameters, _clientContext.Parameters).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    throw new BoltClientException(
                        $"Failed to serialize parameters for action '{_clientContext.Action.Name}'.",
                        ClientErrorCode.SerializeParameters,
                        _clientContext.Action.Name,
                        e);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = -1;
                return false;
            }
        }
    }
}
