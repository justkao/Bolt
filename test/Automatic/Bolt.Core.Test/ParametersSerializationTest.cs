using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

namespace Bolt.Core.Test
{
    public class ParametersSerializationTest
    {
        public ParametersSerializationTest()
        {
            Serializer = new JsonSerializer();
        }

        public ISerializer Serializer { get; set; }

        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(0)]
        [Theory]
        public void WriteSingleArgument_OK(int value)
        {
            MemoryStream stream = new MemoryStream();
            var input = new object[Args3Parameters.GetParameters().Length];
            var output = new object[Args3Parameters.GetParameters().Length];
            input[0] = value;

            Serializer.Write(stream, Args3Parameters, input);

            stream.Seek(0, SeekOrigin.Begin);
            Serializer.Read(stream, Args3Parameters, output);

            Assert.Equal(value, output[0]);
        }

        [InlineData(null, "description")]
        [InlineData("name", "description")]
        [InlineData("name", null)]
        [InlineData(null, null)]
        [Theory]
        public void WriteSingleComplexArgument_OK(string name, string description)
        {
            SimpleObject obj = null;
            if (name != null || description != null)
            {
                obj = new SimpleObject()
                {
                    Name = name,
                    Description = description
                };
            }

            MemoryStream stream = new MemoryStream();
            var input = new object[Args2Parameters.GetParameters().Length];
            var output = new object[Args2Parameters.GetParameters().Length];
            input[0] = obj;

            Serializer.Write(stream, Args2Parameters, input);

            stream.Seek(0, SeekOrigin.Begin);
            Serializer.Read(stream, Args2Parameters, output);

            Assert.Equal(input[0], output[0]);

        }

        [InlineData(1, "name", 1.0, "objname")]
        [InlineData(1, null, 1.0, "objname")]
        [InlineData(1, null, 1.0, null)]
        [InlineData(1, "name", 1.0, null)]
        [InlineData(33, "aaaname", 5454.0, "objnamesss")]
        [Theory]
        public void WriteMultipleParameters_OK(int arg1, string arg2, double arg3, string arg4)
        {
            MemoryStream stream = new MemoryStream();
            var input = new object[Args1Parameters.GetParameters().Length];
            var output = new object[Args1Parameters.GetParameters().Length];
            input[0] = arg1;
            input[1] = arg2;
            input[2] = arg3;
            input[3] = arg4 != null ? new SimpleObject {Name = arg4} : null;

            Serializer.Write(stream, Args1Parameters, input);

            stream.Seek(0, SeekOrigin.Begin);
            Serializer.Read(stream, Args1Parameters, output);

            for (int i = 0; i < input.Length; i++)
            {
                Assert.Equal(input[i], output[i]);
            }
        }

        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [Theory]
        public void WriteParameter_IncludeCAncellationToken_Ok(int arg1)
        {
            MemoryStream stream = new MemoryStream();
            var input = new object[WithCancellationTokenParameters.GetParameters().Length];
            var output = new object[WithCancellationTokenParameters.GetParameters().Length];
            input[0] = arg1;

            Serializer.Write(stream, WithCancellationTokenParameters, input);

            stream.Seek(0, SeekOrigin.Begin);
            Serializer.Read(stream, WithCancellationTokenParameters, output);

            Assert.Equal(input[0], output[0]);
        }

        public class SimpleObject
        {
            public string Name { get; set; }

            public string Description { get; set; }

            protected bool Equals(SimpleObject other)
            {
                return string.Equals(Name, other.Name) && string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SimpleObject) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Description != null ? Description.GetHashCode() : 0);
                }
            }
        }

        private static MethodInfo Args1Parameters =
            typeof (ParametersSerializationTest).GetRuntimeMethods().First(m => m.Name == nameof(Args1));

        private static MethodInfo Args2Parameters =
            typeof (ParametersSerializationTest).GetRuntimeMethods().First(m => m.Name == nameof(Args2));

        private static MethodInfo Args3Parameters =
            typeof (ParametersSerializationTest).GetRuntimeMethods().First(m => m.Name == nameof(Args3));

        private static MethodInfo WithCancellationTokenParameters =
            typeof (ParametersSerializationTest).GetRuntimeMethods().First(m => m.Name == nameof(WithCancellationToken));

        private void Args1(int arg1, string arg2,  double arg3, SimpleObject arg4)
        {
        }

        private void Args2(SimpleObject arg3)
        {
        }

        private void Args3(int val)
        {
        }

        private void WithCancellationToken(int arg, CancellationToken cancellation)
        {
        }
    }
}
