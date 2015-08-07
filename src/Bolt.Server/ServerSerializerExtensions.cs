using System;
using System.IO;
using System.Reflection;

using Bolt.Core;

namespace Bolt.Server
{
    public static class ServerSerializerExtensions
    {
        public static IObjectDeserializer DeserializeParameters(this ISerializer serializer, Stream stream, MethodInfo action)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new DeserializeParametersException($"The data required to deserialize  parameters for action '{action.Name}' are not available in request.");
            }

            try
            {
                return serializer.CreateDeserializer(stream);
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

                throw new DeserializeParametersException($"Failed to deserialize parameters for action '{action.Name}'.",e);
            }
        }

        public static object ReadParameterValue(this IObjectDeserializer serializer, MethodInfo action, string key, Type parameterType)
        {
            try
            {
                return serializer.GetValue(key, parameterType);
            }
            catch (DeserializeParametersException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DeserializeParametersException($"Failed to deserialize parameter '{key}' for action '{action.Name}'.", e);
            }
        }

        public static MemoryStream SerializeResponse(this ISerializer serializer, object data, MethodInfo action)
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
                throw new SerializeResponseException($"Failed to serialize response data for action '{action.Name}'.", e);
            }
        }
    }
}