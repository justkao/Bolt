using NUnit.Framework;
using System;
using System.IO;

namespace Bolt.Core.Serialization.Test
{
    [TestFixture]
    public class ExceptionSerializerTest
    {
        [Test]
        public void Serialize_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null, null));
        }

        [Test]
        public void Desserialize_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null));
        }

        [Test]
        public void DeserializeException_EnsureStackTracePreserved()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray()));

            Assert.IsNotNullOrEmpty(deserialized.StackTrace);
        }

        [Test]
        public void DeserializeException_EnsureMessagePreserved()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray()));

            Assert.AreEqual("custom message", deserialized.Message);
        }

        [Test]
        public void DeserializeException_EnsureTypePreserved()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray()));

            Assert.IsInstanceOf<CustomException>(deserialized);
        }

        [Test]
        public void DeserializeException_EnsureCustomDataPreserved()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException(10)));
            CustomException deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray())) as CustomException;

            Assert.AreEqual(10, deserialized.CustomData);
        }


        [Test]
        public void DeserializeException_EnsureInnerExceptionTypePreserverd()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException("test", Exception(new CustomException()))));
            CustomException deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray())) as CustomException;

            Assert.IsInstanceOf<CustomException>(deserialized.InnerException);
        }

        [Test]
        public void DeserializeException_EnsureInnerExceptionMessagePreserverd()
        {
            MemoryStream raw = new MemoryStream();
            _serializer.Serialize(raw, Exception(new CustomException("test", Exception(new CustomException("test2")))));
            CustomException deserialized = _serializer.Deserialize(new MemoryStream(raw.ToArray())) as CustomException;

            Assert.AreEqual("test2", deserialized.InnerException.Message);
        }

        private Exception Exception(CustomException e)
        {
            try
            {
                throw e;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private IExceptionSerializer _serializer = new JsonExceptionSerializer(new JsonSerializer());
    }
}
