using System;
using Bolt.Serialization;
using Bolt.Test.Common;
using Xunit;

namespace Bolt.Core.Test
{
    public class JsonExceptionWrapperTest
    {
        public JsonExceptionWrapperTest()
        {
            Subject = new JsonExceptionSerializer();
        }

        public JsonExceptionSerializer Subject { get; set; }

        [Fact]
        public void UnwrapException_Ok()
        {
            Exception ex = new Exception("test message");
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.NotNull(deserializedException);
        }

        [Fact]
        public void Unwrap_MessageOk()
        {
            Exception ex = new Exception("test message");
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.Equal(ex.Message, deserializedException.Message);
        }

        [Fact]
        public void Unwrap_TypeOk()
        {
            Exception ex = new InvalidOperationException("test message");
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.IsType<InvalidOperationException>(deserializedException);
        }

        [Fact]
        public void Unwrap_CustomDataOk()
        {
            Exception ex = new CustomException(55);
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.Equal(55, ((CustomException) deserializedException).CustomData);
        }

        [Fact]
        public void Unwrap_InnerExceptionNotNull()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.NotNull(deserializedException.InnerException);
        }

        [Fact]
        public void Unwrap_InnerExceptionMessagePreserved()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var serializedException = Subject.Write(ex);
            var deserializedException = Subject.Read(serializedException);

            Assert.Equal(ex.InnerException.Message, deserializedException.InnerException.Message);
        }
    }
}
