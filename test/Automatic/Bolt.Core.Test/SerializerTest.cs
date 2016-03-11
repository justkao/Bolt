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
            return Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(new WriteValueContext(null, null, null)));
        }

        [Fact]
        public async Task Read_NullObject_DoesNotThrow()
        {
            // arrange
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), null));

            // act 
            CompositeType result =
                (CompositeType)
                    await Serializer.ReadAsync(new ReadValueContext(new MemoryStream(stream.ToArray()), new TestActionContext(), typeof (CompositeType)));

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), obj));

            // act 
            CompositeType result =
                (CompositeType)
                    await Serializer.ReadAsync(new ReadValueContext(new MemoryStream(stream.ToArray()), new TestActionContext(), typeof(CompositeType)));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            // arrange
            SimpleCustomType obj = new SimpleCustomType { BoolProperty = false };
            MemoryStream stream = new MemoryStream();

            // act
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), obj));
            SimpleCustomType deserialized =
                (SimpleCustomType) (await
                    Serializer.ReadAsync(new ReadValueContext(new MemoryStream(stream.ToArray()),
                        new TestActionContext(), typeof (SimpleCustomType))));

            // assert
            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public async Task WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), obj));
            stream.Seek(0, SeekOrigin.Begin);

            // act 
            CompositeType result =
                (CompositeType)
                    await Serializer.ReadAsync(new ReadValueContext(stream, new TestActionContext(), typeof(CompositeType)));

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
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), obj));
            int deserialized =
                (int)(await
                    Serializer.ReadAsync(new ReadValueContext(new MemoryStream(stream.ToArray()),
                        new TestActionContext(), typeof(int))));

            // assert
            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public async Task ReadWrite_SimpleList_EnsureValidResult()
        {
            // arrange
            List<int> obj = new List<int>() {1, 2, 3};
            MemoryStream stream = new MemoryStream();

            // act
            await Serializer.WriteAsync(new WriteValueContext(stream, new TestActionContext(), obj));
            List<int> deserialized =
                (List<int>)(await
                    Serializer.ReadAsync(new ReadValueContext(new MemoryStream(stream.ToArray()),
                        new TestActionContext(), typeof(List<int>))));

            // assert
            Assert.Equal(obj, deserialized);
        }
    }
}
