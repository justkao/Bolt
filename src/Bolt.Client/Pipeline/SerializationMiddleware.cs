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

namespace Bolt.Client.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ClientActionContext>
    {
        private readonly MediaTypeWithQualityHeaderValue _acceptHeader;

        public SerializationMiddleware(ISerializer serializer, IExceptionWrapper exceptionWrapper,
            IClientErrorProvider errorProvider)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (exceptionWrapper == null) throw new ArgumentNullException(nameof(exceptionWrapper));
            if (errorProvider == null) throw new ArgumentNullException(nameof(errorProvider));

            Serializer = serializer;
            ExceptionWrapper = exceptionWrapper;
            ErrorProvider = errorProvider;

            _acceptHeader = new MediaTypeWithQualityHeaderValue(Serializer.MediaType);
        }

        public ISerializer Serializer { get; }

        public IExceptionWrapper ExceptionWrapper { get; }

        public IClientErrorProvider ErrorProvider { get; }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.Request.Headers.Accept.Add(_acceptHeader);
            if (context.EnsureRequest().Content == null)
            {
                context.EnsureRequest().Content = BuildRequestContent(context);
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

        protected virtual HttpContent BuildRequestContent(ClientActionContext context)
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

            var serializeContext = CreateSerializeContext(context);
            if (serializeContext?.Values != null)
            {
                return new SerializeParametersContent(serializeContext, Serializer, context);
            }

            return null;
        }

        private SerializeContext CreateSerializeContext(ClientActionContext context)
        {
            SerializeContext ctxt = new SerializeContext()
            {
                Values = new List<KeyValuePair<string, object>>()
            };

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

        private class SerializeParametersContent : HttpContent
        {
            private readonly ClientActionContext _clientContext;
            private readonly SerializeContext _context;
            private readonly ISerializer _serializer;

            public SerializeParametersContent(SerializeContext context, ISerializer serializer, ClientActionContext clientContext)
            {
                _context = context;
                _serializer = serializer;
                _clientContext = clientContext;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                _context.Stream = stream;

                try
                {
                    await _serializer.WriteAsync(_context);
                }
                catch (Exception e)
                {
                    throw new BoltClientException(
                        $"Failed to serialize parameters for action '{_clientContext.Action.Name}'.",
                        ClientErrorCode.SerializeParameters,
                        _clientContext.Action,
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
