using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Bolt.Metadata;

namespace Bolt.Serialization
{
    public abstract class SerializerBase : ISerializer
    {
        public SerializerBase(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                throw new ArgumentException("The serializer media type needs to be specified", nameof(mediaType));
            }

            MediaType = mediaType;
        }

        public string MediaType { get; }

        public Task WriteAsync(Stream stream, Type type, object value, Action<long> onContentLength)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return DoWriteAsync(stream, type, value, onContentLength);
        }

        public Task<object> ReadAsync(Stream stream, Type valueType, long contentLength)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            return DoReadAsync(stream, valueType, contentLength);
        }

        public Task WriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (parameters == null || parameters.Count == 0)
            {
                return Task.CompletedTask;
            }

            ActionMetadata.ValidateParameters(parameters, values);

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameterMetadata = parameters[i];
                if (!parameterMetadata.IsSerializable)
                {
                    values[i] = null;
                }
            }

            return DoWriteParametersAsync(stream, parameters, values, onContentLength);
        }

        public async Task<object[]> ReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (parameters == null || parameters.Count == 0)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            object[] values = await DoReadParametersAsync(stream, parameters, contentLength).ConfigureAwait(false);

            for (int i = 0; i < values.Length; i++)
            {
                var rawValue = values[i];
                if (rawValue != null)
                {
                    if (Convert.GetTypeCode(rawValue) != TypeCode.Object)
                    {
                        rawValue = Convert.ChangeType(rawValue, parameters[i].Type, CultureInfo.InvariantCulture);
                    }
                }

                values[i] = rawValue;
            }

            ActionMetadata.ValidateParameters(parameters, values);
            return values;
        }

        protected abstract Task DoWriteAsync(Stream stream, Type type, object value, Action<long> onContentLength);

        protected abstract Task<object> DoReadAsync(Stream stream, Type valueType, long contentLength);

        protected abstract Task DoWriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength);

        protected abstract Task<object[]> DoReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength);
    }
}