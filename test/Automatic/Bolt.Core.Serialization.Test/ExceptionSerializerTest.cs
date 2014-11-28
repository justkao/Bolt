using NUnit.Framework;
using System;

namespace Bolt.Core.Serialization.Test
{
    [TestFixture]
    public class ExceptionSerializerTest
    {
        [Test]
        public void Serialize_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null));
        }

        [Test]
        public void Desserialize_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null));
        }

        [Test]
        public void DeserializeException_EnsureStackTracePreserved()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(raw);

            Assert.IsNotNullOrEmpty(deserialized.StackTrace);
        }

        [Test]
        public void DeserializeException_EnsureMessagePreserved()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(raw);

            Assert.AreEqual("custom message", deserialized.Message);
        }

        [Test]
        public void DeserializeException_EnsureTypePreserved()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Deserialize(raw);

            Assert.IsInstanceOf<CustomException>(deserialized);
        }

        [Test]
        public void DeserializeException_EnsureCustomDataPreserved()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException(10)));
            CustomException deserialized = _serializer.Deserialize(raw) as CustomException;

            Assert.AreEqual(10, deserialized.CustomData);
        }


        [Test]
        public void DeserializeException_EnsureInnerExceptionTypePreserverd()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException("test", Exception(new CustomException()))));
            CustomException deserialized = _serializer.Deserialize(raw) as CustomException;

            Assert.IsInstanceOf<CustomException>(deserialized.InnerException);
        }

        [Test]
        public void DeserializeException_EnsureInnerExceptionMessagePreserverd()
        {
            byte[] raw = _serializer.Serialize(Exception(new CustomException("test", Exception(new CustomException("test2")))));
            CustomException deserialized = _serializer.Deserialize(raw) as CustomException;

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

        private IExceptionSerializer _serializer = new JsonExceptionSerializer();
    }
}
