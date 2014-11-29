using System;
using System.IO;

namespace Bolt.Client
{
    public static class ClientSerializerExtensions
    {
        public static byte[] SerializeParameters<TParameters>(this ISerializer serializer, TParameters parameters, ActionDescriptor actionDescriptor)
        {
            if (typeof(TParameters) == typeof(Empty))
            {
                return null;
            }

            if (Equals(parameters, null))
            {
                return null;
            }

            try
            {
                return serializer.Serialize(parameters);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SerializeParametersException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new SerializeParametersException(
                    string.Format("Failed to serialize parameters for action '{0}'. Parameters type - '{1}'", actionDescriptor, typeof(TParameters).FullName),
                    e);
            }
        }

        public static T DeserializeResponse<T>(this ISerializer serializer, Stream stream, ActionDescriptor actionDescriptor)
        {
            if (stream == null || stream.Length == 0)
            {
                return default(T);
            }

            try
            {
                return serializer.Read<T>(stream);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DeserializeResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new DeserializeResponseException(
                    string.Format("Failed to deserialize response data for action '{0}'.", actionDescriptor), e);
            }
        }

        public static Exception DeserializeExceptionResponse(this IExceptionSerializer serializer, Stream rawException, ActionDescriptor actionDescriptor)
        {
            if (rawException == null)
            {
                return null;
            }

            try
            {
                return serializer.Deserialize(rawException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DeserializeResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();

                throw new DeserializeResponseException(
                    string.Format("Failed to deserialize exception response for action '{0}'.", actionDescriptor), e);
            }
        }
    }
}