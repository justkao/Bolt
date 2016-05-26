using System;
using Bolt.Serialization;
using Bolt.Test.Common;
using Xunit;

namespace Bolt.Core.Test
{
#if NET451
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
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.NotNull(deserializedException);
        }

        [Fact]
        public void Unwrap_MessageOk()
        {
            Exception ex = new Exception("test message");
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.Equal(ex.Message, deserializedException.Message);
        }

        [Fact]
        public void Unwrap_TypeOk()
        {
            Exception ex = new InvalidOperationException("test message");
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.IsType<InvalidOperationException>(deserializedException);
        }

        [Fact]
        public void Unwrap_CustomDataOk()
        {
            Exception ex = new CustomException(55);
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.Equal(55, ((CustomException) deserializedException).CustomData);
        }

        [Fact]
        public void Unwrap_InnerExceptionNotNull()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.NotNull(deserializedException.InnerException);
        }

        [Fact]
        public void Unwrap_InnerExceptionMessagePreserved()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var writeContext = new WriteExceptionContext(new TestActionContext(), ex);
            var serializedException = Subject.Write(writeContext);
            var readContext = new ReadExceptionContext(new TestActionContext(), serializedException);
            var deserializedException = Subject.Read(readContext);

            Assert.Equal(ex.InnerException.Message, deserializedException.InnerException.Message);
        }
    }
#endif
}
