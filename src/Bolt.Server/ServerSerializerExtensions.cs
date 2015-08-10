using System;
using System.IO;
using System.Reflection;

using Bolt.Core;

namespace Bolt.Server
{
    public static class ServerSerializerExtensions
    {
        public static IObjectSerializer DeserializeParameters(this ISerializer serializer, Stream stream, MethodInfo action)
        {
            try
            {
                return serializer.CreateSerializer(stream);
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

        public static object ReadParameterValue(this IObjectSerializer serializer, MethodInfo action, string key, Type parameterType)
        {
            try
            {
                object value;
                serializer.TryRead(key, parameterType, out value);
                return value;
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