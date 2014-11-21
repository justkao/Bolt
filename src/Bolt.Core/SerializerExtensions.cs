using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    public static class SerializerExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this ISerializer serializer, Stream stream, bool bufferStream, CancellationToken cancellation)
        {
            if (stream == null)
            {
                return default(T);
            }

            if (bufferStream)
            {
                MemoryStream buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, 4096, cancellation);
                buffer.Seek(0, SeekOrigin.Begin);
                stream.Dispose();
                stream = buffer;
            }

            try
            {
                return serializer.Read<T>(stream);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                throw new SerializationException(string.Format("Failed to deserialize object '{0}'.", typeof(T).Name), e);
            }
            finally
            {
                if (bufferStream)
                {
                    stream.Dispose();
                }
            }
        }

        public static T Deserialize<T>(this ISerializer serializer, Stream stream, bool bufferStream)
        {
            if (stream == null)
            {
                return default(T);
            }

            if (bufferStream)
            {
                MemoryStream buffer = new MemoryStream();
                stream.CopyTo(buffer);
                buffer.Seek(0, SeekOrigin.Begin);
                stream.Dispose();
                stream = buffer;
            }

            try
            {
                return serializer.Read<T>(stream);
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                throw new SerializationException(string.Format("Failed to deserialize object '{0}'.", typeof(T).Name), e);
            }
            finally
            {
                if (bufferStream)
                {
                    stream.Dispose();
                }
            }
        }

        public static byte[] Serialize<T>(this ISerializer serializer, T data)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Write(stream, data);
                    return stream.ToArray();
                }
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SerializationException(string.Format("Failed to serialize object '{0}'.", typeof(T).Name), e);
            }
        }
    }
}