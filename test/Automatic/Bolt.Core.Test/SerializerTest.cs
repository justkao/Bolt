using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bolt.Serialization;
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
        public async Task Read_NullObject_DoesNotThrow()
        {
            // arrange
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, null);

            // act 
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(CompositeType));

            // assert
            Assert.Null(result);
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
    }
}
