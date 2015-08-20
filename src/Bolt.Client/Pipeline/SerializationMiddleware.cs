using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        public override async Task InvokeAsync(ClientActionContext context)
        {
            if (context.EnsureRequest().Content == null && context.HasParameters)
            {
                context.EnsureRequest().Content = BuildRequestParameters(context);
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
                    errorOnServer = DeserializeException(context, stream);
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
                if (context.HasSerializableActionResult && context.ActionResult == null)
                {
                    using (Stream stream = await GetResponseStreamAsync(context.Response))
                    {
                        context.ActionResult = DeserializeResponse(context, stream);
                    }
                }
            }
        }

        protected virtual async Task<Stream> GetResponseStreamAsync(HttpResponseMessage response)
        {
            return new MemoryStream(await response.Content.ReadAsByteArrayAsync());
        }

        protected virtual HttpContent BuildRequestParameters(ClientActionContext context)
        {
            try
            {
                BoltFramework.ValidateParameters(context.Action, context.Parameters);
            }
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Parameter validation failed for action '{context.Action.Name}'.",
                    ClientErrorCode.SerializeParameters,
                    context.Action,
                    e);
            }

            IObjectSerializer parameterSerializer = Serializer.CreateSerializer();
            ParameterInfo[] parameterTypes = context.Action.GetParameters();
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

                try
                {
                    parameterSerializer.Write(parameterTypes[i].Name, context.Parameters[i].GetType(), context.Parameters[i]);
                }
                catch (BoltException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new BoltClientException(
                        $"Failed to serialize value for parameter '{parameterTypes[i].Name}' and action '{context.Action.Name}'.",
                        ClientErrorCode.SerializeParameters,
                        context.Action,
                        e);
                }
            }

            if (parameterSerializer.IsEmpty)
            {
                return null;
            }

            try
            {
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
            catch (Exception e)
            {
                throw new BoltClientException(
                    $"Failed to serialize parameters for action '{context.Action.Name}'.",
                    ClientErrorCode.SerializeParameters,
                    context.Action,
                    e);
            }
        }

        protected virtual object DeserializeResponse(ClientActionContext context, Stream stream)
        {
            if (stream.Length == 0)
            {
                return null;
            }

            try
            {
                return Serializer.Read(context.ResponseType, stream);
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

        protected virtual Exception DeserializeException(ClientActionContext context, Stream stream)
        {
            if (stream.Length == 0)
            {
                return null;
            }

            try
            {
                object result = Serializer.Read(ExceptionWrapper.Type, stream);
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
