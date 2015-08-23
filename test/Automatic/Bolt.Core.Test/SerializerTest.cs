using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public void Write_NullStream_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Serializer.Write(null, null));
        }

        [Fact]
        public void Read_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Serializer.Read<string>(null));
        }

        [Fact]
        public void Write_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, null);
        }

        [Fact]
        public void WriteString_EnsureNoRedundancy()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, "test");

            Assert.Equal("test", Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Fact]
        public void Read_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, null);

            CompositeType result = Serializer.Read<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.Null(result);
        }

        [Fact]
        public void WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            CompositeType deserialized = Serializer.Read<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public void WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            SimpleCustomType obj = new SimpleCustomType { BoolProperty = false };
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            SimpleCustomType deserialized = Serializer.Read<SimpleCustomType>(new MemoryStream(stream.ToArray()));

            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public void WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);
            CompositeType deserialized = Serializer.Read<CompositeType>(stream);

            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public void ReadWrite_SimpleType_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, 10);

            var result = Serializer.Read<int>(new MemoryStream(stream.ToArray()));
            Assert.Equal(10, result);
        }

        [Fact]
        public void ReadWrite_SimpleList_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, new List<int> { 1, 2, 3 });

            var result = Serializer.Read<List<int>>(new MemoryStream(stream.ToArray()));
            Assert.Equal(result[0], 1);
            Assert.Equal(result[1], 2);
            Assert.Equal(result[2], 3);
        }
    }
}
