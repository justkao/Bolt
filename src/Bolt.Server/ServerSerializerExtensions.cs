using System;
using System.IO;

namespace Bolt.Server
{
    public static class ServerSerializerExtensions
    {
        public static object DeserializeParameters(this ISerializer serializer, Stream stream, ActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.Parameters == typeof(Empty))
            {
                return null;
            }

            if (stream == null || stream.Length == 0)
            {
                throw new DeserializeParametersException($"The data required to deserialize '{actionDescriptor.Parameters.Name}' parameters for action '{actionDescriptor}' are not available in request.");
            }

            try
            {
                return serializer.Read(actionDescriptor.Parameters, stream);
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

                throw new DeserializeParametersException($"Failed to deserialize parameters for action '{actionDescriptor}'. Parameters type - '{actionDescriptor.Parameters.FullName}'",e);
            }
        }

        public static byte[] SerializeResponse(this ISerializer serializer, object data, ActionDescriptor actionDescriptor)
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