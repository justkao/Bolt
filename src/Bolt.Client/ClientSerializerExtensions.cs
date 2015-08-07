using System;
using System.IO;
using System.Reflection;

using Bolt.Core;

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
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="action">The descriptor for action that is using the parameters class.</param>
        /// <returns>The serialized parameters or null.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="SerializeParametersException">Thrown if any error occurred during serialization.</exception>
        public static Stream SerializeParameters(this IObjectSerializer serializer, MethodInfo action)
        {
            try
            {
                return serializer.Serialize();
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
                throw new SerializeParametersException($"Failed to serialize parameters for action '{action.Name}'.", e);
            }
        }

        /// <summary>
        /// Deserialize the server response into the concrete resultType.
        /// </summary>
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="resultType">Expected result type.</param>
        /// <param name="stream">The stream used to deserialize the data.</param>
        /// <param name="action">The action context of deserialize operation.</param>
        /// <returns>The deserialized data or default(T) if stream is null or empty.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="DeserializeResponseException">Thrown if any error occurred during deserialization.</exception>
        public static object DeserializeResponse(this ISerializer serializer, Type resultType, Stream stream, MethodInfo action)
        {
            if (resultType == typeof (Empty))
            {
                return Empty.Instance;
            }

            if (stream == null || stream.Length == 0)
            {
                return null;
            }

            try
            {
                return serializer.Read(resultType, stream);
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
                throw new DeserializeResponseException($"Failed to deserialize response data for action '{action.Name}'.", e);
            }
        }

        /// <summary>
        /// Deserialize the server response into the concrete resultType.
        /// </summary>
        /// <param name="serializer">The data serializer instance.</param>
        /// <param name="type">The resultType of data to deserialize.</param>
        /// <param name="stream">The stream used to deserialize the data.</param>
        /// <param name="action">The action context of deserialize operation.</param>
        /// <returns>The deserialized data or default(T) if stream is null or empty.</returns>
        /// <exception cref="TimeoutException">Thrown if timeout occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation was cancelled.</exception>
        /// <exception cref="DeserializeResponseException">Thrown if any error occurred during deserialization.</exception>
        public static object DeserializeExceptionResponse(this ISerializer serializer, Type type, Stream stream, MethodInfo action)
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
                throw new DeserializeResponseException($"Failed to deserialize exception response data for action '{action.Name}'.", e);
            }
        }
    }
}