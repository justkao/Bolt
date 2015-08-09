using Bolt.Test.Common;
using System;
using System.IO;
using Xunit;

namespace Bolt.Core.Test
{
    public class ExceptionWrapperTest
    {
        private readonly IExceptionWrapper _serializer;

        public ExceptionWrapperTest()
        {
            _serializer = new JsonExceptionWrapper();
        }

        [Fact]
        [InlineData()]
        public void Wrap_NullArgument_ReturnsNull()
        {
            Assert.Null(_serializer.Wrap(null));
        }

        [Fact]
        public void Unwrap_NullArgument_ReturnsNull()
        {
            Assert.Null(_serializer.Unwrap(null));
        }

        [Fact]
        public void WrapException_EnsureStackTracePreserved()
        {
            var result = _serializer.Wrap(Exception(new CustomException("test")));
            Exception deserialized = _serializer.Unwrap(result);

            Assert.NotEmpty(deserialized.StackTrace);
        }

        [Fact]
        public void Unwrap_EnsureMessagePreserved()
        {
            MemoryStream raw = new MemoryStream();
            var result = _serializer.Wrap(Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Unwrap(result);

            Assert.Equal("custom message", deserialized.Message);
        }

        [Fact]
        public void Unwrap_EnsureTypePreserved()
        {
            var result = _serializer.Wrap(Exception(new CustomException("custom message")));
            Exception deserialized = _serializer.Unwrap(result);

            Assert.IsType<CustomException>(deserialized);
        }

        [Fact]
        public void Unwrap_EnsureCustomDataPreserved()
        {
            MemoryStream raw = new MemoryStream();
            var result = _serializer.Wrap(Exception(new CustomException(10)));
            CustomException deserialized = _serializer.Unwrap(result) as CustomException;

            Assert.Equal(10, deserialized.CustomData);
        }

        [Fact]
        public void Unwrap_EnsureInnerExceptionTypePreserverd()
        {
            var result = _serializer.Wrap(Exception(new CustomException("test", Exception(new CustomException()))));
            CustomException deserialized = _serializer.Unwrap(result) as CustomException;

            Assert.IsType<CustomException>(deserialized.InnerException);
        }

        [Fact]
        public void Unwrap_EnsureInnerExceptionMessagePreserverd()
        {
            var result = _serializer.Wrap(Exception(new CustomException("test", Exception(new CustomException("test2")))));
            CustomException deserialized = _serializer.Unwrap(result) as CustomException;

            Assert.Equal("test2", deserialized.InnerException.Message);
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
    }
}
