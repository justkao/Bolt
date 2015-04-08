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
                throw new DeserializeParametersException($"The data required to deserialize '{typeof (TParameters).Name}' parameters for action '{actionDescriptor}' are not available in request.");
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

                throw new DeserializeParametersException($"Failed to deserialize parameters for action '{actionDescriptor}'. Parameters type - '{typeof (TParameters).FullName}'",e);
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
                throw new SerializeResponseException($"Failed to serialize response data for action '{actionDescriptor}'.", e);
            }
        }
    }
}