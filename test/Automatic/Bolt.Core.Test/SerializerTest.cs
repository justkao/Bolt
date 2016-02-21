using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Bolt.Test.Common;
using Xunit;

namespace Bolt.Core.Test
{
    public class JsonSerializerTest
    {
        public JsonSerializerTest()
        {
            Serializer = new JsonSerializer();
        }

        public ISerializer Serializer { get; }

        [Fact]
        public Task Write_NullStream_ThrowsArgumentNullException()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(null, null));
        }

        [Fact]
        public Task Read_NullArgument_ThrowsArgumentNullException()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.ReadAsync<string>(null));
        }

        [Fact]
        public Task Write_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            return Serializer.WriteAsync(stream, null);
        }

        [Fact]
        public async Task Read_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, null);

            CompositeType result = await Serializer.ReadAsync<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.Null(result);
        }

        [Fact]
        public async Task WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);
            CompositeType deserialized = await Serializer.ReadAsync<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public async Task WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            SimpleCustomType obj = new SimpleCustomType { BoolProperty = false };
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);
            SimpleCustomType deserialized = await Serializer.ReadAsync<SimpleCustomType>(new MemoryStream(stream.ToArray()));

            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public async Task WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);
            CompositeType deserialized = await Serializer.ReadAsync<CompositeType>(stream);

            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public async Task ReadWrite_SimpleType_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, 10);

            var result = await Serializer.ReadAsync<int>(new MemoryStream(stream.ToArray()));
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task ReadWrite_SimpleList_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, new List<int> { 1, 2, 3 });

            var result = await Serializer.ReadAsync<List<int>>(new MemoryStream(stream.ToArray()));
            Assert.Equal(result[0], 1);
            Assert.Equal(result[1], 2);
            Assert.Equal(result[2], 3);
        }
    }
}
