using Bolt.Service.Test;
using Bolt.Service.Test.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Core.Serialization.Test
{
    public class DataSerializerTest : SerializerTestBase
    {
        public DataSerializerTest(SerializerType serializerType)
            : base(serializerType)
        {
        }

        [Test]
        public void Write_NullStream_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Serializer.Write<string>(null, null));
        }

        [Test]
        public void Read_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Serializer.Read<string>(null));
        }

        [Test]
        public void Write_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write<CompositeType>(stream, null);
        }

        [Test]
        public void Read_NullObject_DoesNotThrow()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write<CompositeType>(stream, null);

            CompositeType result = Serializer.Read<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.IsNull(result);
        }

        [Test]
        public void WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            CompositeType deserialized = Serializer.Read<CompositeType>(new MemoryStream(stream.ToArray()));
            Assert.AreEqual(obj, deserialized);
        }

        [Test]
        public void WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            SimpleCustomType obj = new SimpleCustomType() { BoolProperty = false };
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            SimpleCustomType deserialized = Serializer.Read<SimpleCustomType>(new MemoryStream(stream.ToArray()));

            Assert.AreEqual(obj, deserialized);
        }

        [Test]
        public void WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            Serializer.Write(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);
            CompositeType deserialized = Serializer.Read<CompositeType>(stream);

            Assert.AreEqual(obj, deserialized);
        }

        [Test]
        public void ReadWrite_SimpleType_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write<int>(stream, 10);

            var result = Serializer.Read<int>(new MemoryStream(stream.ToArray()));
            Assert.AreEqual(10, result);
        }

        [Test]
        public void ReadWrite_SimpleList_EnsureValidResult()
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Write<List<int>>(stream, new List<int>() { 1, 2, 3 });

            var result = Serializer.Read<List<int>>(new MemoryStream(stream.ToArray()));
            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[1], 2);
            Assert.AreEqual(result[2], 3);
        }

    }
}