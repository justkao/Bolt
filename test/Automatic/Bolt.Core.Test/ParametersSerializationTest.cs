using Bolt.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bolt.Core.Test
{
    public class ParametersSerializationTest
    {
        public ISerializer Serializer { get; } = new JsonSerializer();

        [InlineData(11.1)]
        [InlineData("value")]
        [InlineData(10)]
        [InlineData(true)]
        [Theory]
        public Task WriteSimpleParameter_EnsureDeserializedProperly(object parameter)
        {
            return WriteSimpleParameterHelper(parameter, parameter);
        }

        [MemberData(nameof(GetManyProperties), 1)]
        [MemberData(nameof(GetManyProperties), 2)]
        [MemberData(nameof(GetManyProperties), 10)]
        [Theory]
        public async Task WriteSimpleParameters_EnsureDeserializedProperly((ParameterMetadata metadata, object value)[] parameters)
        {
            MemoryStream output = new MemoryStream();
            var values = parameters.Select(p => p.value).ToArray();
            var metadata = parameters.Select(p => p.metadata).ToArray();
            var outputValues = new object[parameters.Length];

            await Serializer.WriteParametersAsync(output, metadata, values);

            output.Seek(0, SeekOrigin.Begin);
            await Serializer.ReadParametersAsync(output, metadata, outputValues);

            for (int i = 0; i < parameters.Length; i++)
            {
                Assert.Equal(parameters[i].value, outputValues[i]);
            }
        }

        [Fact]
        public Task WriteCancellationTokenParameter_ShouldBeSkipped()
        {
            return WriteSimpleParameterHelper(CancellationToken.None, null);
        }

        [InlineData(11.1, null)]
        [InlineData("value", null)]
        [InlineData(10, null)]
        [InlineData(true, null)]
        [Theory]
        public async Task UnknownName_Skipped(object parameter, object expected)
        {
            MemoryStream output = new MemoryStream();
            var parameters1 = new[] { new ParameterMetadata(parameter.GetType(), "name1") };
            var parameters2 = new[] { new ParameterMetadata(parameter.GetType(), "name2") };

            var values = new object[] { parameter };
            var outputValues = new object[1];

            await Serializer.WriteParametersAsync(output, parameters1, values);

            output.Seek(0, SeekOrigin.Begin);
            await Serializer.ReadParametersAsync(output, parameters2, outputValues);

            Assert.Equal(expected, outputValues[0]);
        }

        private async Task WriteSimpleParameterHelper(object parameter, object expected)
        {
            MemoryStream output = new MemoryStream();
            var parameters = new[] { new ParameterMetadata(parameter.GetType(), "name") };
            var values = new object[] { parameter };
            var outputValues = new object[1];

            await Serializer.WriteParametersAsync(output, parameters, values);

            output.Seek(0, SeekOrigin.Begin);
            await Serializer.ReadParametersAsync(output, parameters, outputValues);

            Assert.Equal(expected, outputValues[0]);
        }


        public static IEnumerable<object[]> GetManyProperties(int parameters)
        {
            yield return new object[] { Enumerable.Range(0, parameters).Select(index => (new ParameterMetadata(typeof(int), $"name_{index}"), (object)index)).Cast<object>().ToArray() };
        }
    }
}
