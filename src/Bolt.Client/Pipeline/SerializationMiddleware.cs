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
                    context.Action);
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
                if (context.GetActionMetadataOrThrow().HasResult && context.ActionResult == null)
                {
                    using (Stream stream = await GetResponseStreamAsync(context.Response).ConfigureAwait(false))
                    {
                        context.ActionResult = await DeserializeResponseAsync(context, stream).ConfigureAwait(false);
                    }
                }
            }
        }

        protected virtual async Task<Stream> GetResponseStreamAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        protected virtual HttpContent BuildRequestContent(ClientActionContext context)
        {
            ActionMetadata metadata = context.GetActionMetadataOrThrow();
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

            var parameterValues = CreateParameterValues(context);
            if (parameterValues != null)
            {
                return new SerializeParametersContent(parameterValues, Serializer, context);
            }

            return null;
        }

        private List<ParameterValue> CreateParameterValues(ClientActionContext context)
        {
            List<ParameterValue> parameterValues = null;

            for (int i = 0; i < context.ActionMetadata.Parameters.Count; i++)
            {
                if (context.Parameters[i] == null)
                {
                    continue;
                }

                if (context.Parameters[i] is CancellationToken)
                {
                    continue;
                }

                if (parameterValues == null)
                {
                    parameterValues = new List<ParameterValue>();
                }

                parameterValues.Add(new ParameterValue(context.ActionMetadata.Parameters[i], context.Parameters[i]));
            }

            return parameterValues;
        }

        protected virtual async Task<object> DeserializeResponseAsync(ClientActionContext context, Stream stream)
        {
            try
            {
                var readContext = new ReadValueContext(stream, context, context.ActionMetadata.ResultType);
                await Serializer.ReadAsync(readContext).ConfigureAwait(false);
                return readContext.Value;
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
            if (stream == null)
            {
                return null;
            }

            try
            {
                var readContext = new ReadValueContext(stream, context, ExceptionSerializer.Type);
                await Serializer.ReadAsync(readContext).ConfigureAwait(false);
                if (readContext.Value == null)
                {
                    return null;
                }

                return ExceptionSerializer.Read(new ReadExceptionContext(context, readContext.Value));
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
            private readonly List<ParameterValue> _parameters;
            private readonly ISerializer _serializer;

            public SerializeParametersContent(List<ParameterValue> parameters, ISerializer serializer, ClientActionContext clientContext)
            {
                _parameters = parameters;
                _serializer = serializer;
                _clientContext = clientContext;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                try
                {
                    await _serializer.WriteAsync(new WriteParametersContext(stream, _clientContext, _parameters)).ConfigureAwait(false);
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
