using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Test.Common;
using Xunit;

namespace Bolt.Client.Test
{
    public class JsonExceptionWrapperTest
    {
        public JsonExceptionWrapperTest()
        {
            Subject = new JsonExceptionWrapper();
        }

        public JsonExceptionWrapper Subject { get; set; }

        [Fact]
        public void UnwrapException_Ok()
        {
            Exception ex = new Exception("test message");
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.NotNull(copy);
        }

        [Fact]
        public void Unwrap_MessageOk()
        {
            Exception ex = new Exception("test message");
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.Equal(copy.Message, ex.Message);
        }

        [Fact]
        public void Unwrap_TypeOk()
        {
            Exception ex = new InvalidOperationException("test message");
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.IsType<InvalidOperationException>(copy);
        }

        [Fact]
        public void Unwrap_CustomDataOk()
        {
            Exception ex = new CustomException(55);
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.Equal(55, (copy as CustomException).CustomData);
        }

        [Fact]
        public void Unwrap_InnerExceptionNotNull()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.NotNull(copy.InnerException);
        }

        [Fact]
        public void Unwrap_InnerExceptionMessagePreserved()
        {
            Exception ex = new Exception("test", new Exception("test 2"));
            var copy = Subject.Unwrap(Subject.Wrap(ex));

            Assert.Equal(ex.InnerException.Message, copy.InnerException.Message);
        }
    }
}
