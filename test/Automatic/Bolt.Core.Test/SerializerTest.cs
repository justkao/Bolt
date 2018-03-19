using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Serialization;
using Bolt.Serialization.MessagePack;
using Bolt.Test.Common;
using Xunit;

namespace Bolt.Core.Test
{
    public abstract class SerializerTestBase
    {
        public class JsonSerializerTest : SerializerTestBase
        {
            protected override ISerializer CreateSerializer() => new JsonSerializer();
        }

        public class MessagePackSerializerTest : SerializerTestBase
        {
            protected override ISerializer CreateSerializer() => new MessagePackSerializer();
        }

        protected SerializerTestBase()
        {
            Serializer = CreateSerializer();
        }

        protected abstract ISerializer CreateSerializer();

        public ISerializer Serializer { get; }

        [Fact]
        public Task Write_NullStream_ThrowsArgumentNullException()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(null, null));
        }

        [Fact]
        public async Task Write_NullObject_Throws()
        {
            // arrange
            MemoryStream stream = new MemoryStream();
            await Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(stream, null));
        }

        [Fact]
        public async Task WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);

            // act 
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(CompositeType));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            // arrange
            SimpleCustomType obj = new SimpleCustomType { BoolProperty = false };
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);

            // act
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(SimpleCustomType));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);

            // act 
            var result = await Serializer.ReadAsync(stream, typeof(CompositeType));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task ReadWrite_SimpleType_EnsureValidResult()
        {
            // arrange
            int obj = 10;
            MemoryStream stream = new MemoryStream();

            // act
            await Serializer.WriteAsync(stream, obj);
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(int));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task ReadWrite_SimpleList_EnsureValidResult()
        {
            // arrange
            List<int> obj = new List<int>() {1, 2, 3};
            MemoryStream stream = new MemoryStream();

            // act
            await Serializer.WriteAsync(stream, obj);
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(List<int>));

            // assert
            Assert.Equal(obj, result);
        }

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

            await Serializer.WriteParametersAsync(output, metadata, values);

            output.Seek(0, SeekOrigin.Begin);
            var outputValues = await Serializer.ReadParametersAsync(output, metadata);

            for (int i = 0; i < parameters.Length; i++)
            {
                Assert.Equal(parameters[i].value, outputValues[i]);
            }
        }

        [Fact]
        public Task WriteCancellationTokenParameter_ShouldBeNull()
        {
            return WriteSimpleParameterHelper(CancellationToken.None, null);
        }

        private async Task WriteSimpleParameterHelper(object parameter, object expected)
        {
            MemoryStream output = new MemoryStream();
            var parameters = new[] { new ParameterMetadata(parameter.GetType(), "name") };
            var values = new object[] { parameter };

            await Serializer.WriteParametersAsync(output, parameters, values);

            output.Seek(0, SeekOrigin.Begin);
            var outputValues = await Serializer.ReadParametersAsync(output, parameters);

            Assert.Equal(expected, outputValues[0]);
        }


        public static IEnumerable<object[]> GetManyProperties(int parameters)
        {
            yield return new object[] { Enumerable.Range(0, parameters).Select(index => (new ParameterMetadata(typeof(int), $"name_{index}"), (object)index)).Cast<object>().ToArray() };

            yield return new object[] { Enumerable.Range(0, parameters).Select(index => (new ParameterMetadata(typeof(CompositeType), $"name_{index}"), (object)CompositeType.CreateRandom())).Cast<object>().ToArray() };
        }
    }
}
