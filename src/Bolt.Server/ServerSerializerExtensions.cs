using System;
using System.IO;

namespace Bolt.Server
{
    public static class ServerSerializerExtensions
    {
        public static TParameters DeserializeParameters<TParameters>(this ISerializer serializer, Stream stream, ActionDescriptor actionDescriptor)
        {
            if (typeof(TParameters) == typeof(Empty))
            {
                return default(TParameters);
            }

            if (stream == null || stream.Length == 0)
            {
                throw new DeserializeParametersException(
                    string.Format(
                        "The data required to deserialize '{0}' parameters for action '{1}' are not available in request.",
                        typeof(TParameters).Name, actionDescriptor));
            }

            try
            {
                return serializer.Read<TParameters>(stream);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DeserializeParametersException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new DeserializeParametersException(
                    string.Format("Failed to deserialize parameters for action '{0}'. Parameters type - '{1}'",
                        actionDescriptor, typeof(TParameters).FullName), e);
            }
        }

        public static byte[] SerializeResponse<T>(this ISerializer serializer, T data, ActionDescriptor actionDescriptor)
        {
            if (Equals(data, null))
            {
                return null;
            }

            try
            {
                return serializer.Serialize(data);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SerializeResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new SerializeResponseException(
                    string.Format("Failed to serialize response data for action '{0}'.", actionDescriptor), e);
            }
        }

        public static byte[] SerializeExceptionResponse(this IExceptionSerializer serializer, Exception exception, ActionDescriptor actionDescriptor)
        {
            if (Equals(exception, null))
            {
                return null;
            }

            try
            {
                return serializer.Serialize(exception);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SerializeResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new SerializeResponseException(
                    string.Format("Failed to serialize exception response for action '{0}'.", actionDescriptor), e);
            }
        }
    }
}