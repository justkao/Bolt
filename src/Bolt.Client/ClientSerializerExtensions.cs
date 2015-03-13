using System;
using System.IO;

namespace Bolt.Client
{
    /// <summary>
    /// Extensions and helpers for client serialization and deserialization.
    /// </summary>
    public static class ClientSerializerExtensions
    {
        /// <summary>
        /// Serializes the parameters instance into the raw byte array.
        /// </summary>
        /// <typeparam name="TParameters">Type of parameters to serialize.</typeparam>
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="parameters">The instance of parameters. Might be null of <see cref="Empty.Instance"/></param>
        /// <param name="actionDescriptor">The descriptor for action that is using the parameters class.</param>
        /// <returns>The serialized parameters or null.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="SerializeParametersException">Thrown if any error occurred during serialization.</exception>
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
            catch (TimeoutException)
            {
                throw;
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

        /// <summary>
        /// Deserialize the server response into the concrete type.
        /// </summary>
        /// <typeparam name="T">The type of data to deserialize.</typeparam>
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="stream">The stream used to deserialize the data.</param>
        /// <param name="actionDescriptor">The action context of deserialize operation.</param>
        /// <returns>The deserialized data or default(T) if stream is null or empty.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="DeserializeResponseException">Thrown if any error occurred during deserialization.</exception>
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
            catch (TimeoutException)
            {
                throw;
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
                throw new DeserializeResponseException(string.Format("Failed to deserialize response data for action '{0}'.", actionDescriptor), e);
            }
        }

        /// <summary>
        /// Deserialize the server response into the concrete type.
        /// </summary>
        /// <typeparam name="T">The type of data to deserialize.</typeparam>
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="stream">The stream used to deserialize the data.</param>
        /// <param name="actionDescriptor">The action context of deserialize operation.</param>
        /// <returns>The deserialized data or default(T) if stream is null or empty.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="DeserializeResponseException">Thrown if any error occurred during deserialization.</exception>
        public static object DeserializeExceptionResponse(this ISerializer serializer, Type type, Stream stream, ActionDescriptor actionDescriptor)
        {
            if (stream == null || stream.Length == 0)
            {
                return null;
            }

            try
            {
                return serializer.Read(type, stream);
            }
            catch (TimeoutException)
            {
                throw;
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
                throw new DeserializeResponseException(string.Format("Failed to deserialize exception response data for action '{0}'.", actionDescriptor), e);
            }
        }
    }
}