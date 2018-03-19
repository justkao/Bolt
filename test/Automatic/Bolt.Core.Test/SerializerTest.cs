using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Serialization;
using Bolt.Serialization.MessagePack;
using Bolt.Test.Common;
using Xunit;

namespace Bolt.Core.Test
{
    public abstract class SerializerTestBase
    {
        public class JsonSerializerTest : SerializerTestBase
        {
            protected override ISerializer CreateSerializer() => new JsonSerializer();
        }

        public class MessagePackSerializerTest : SerializerTestBase
        {
            protected override ISerializer CreateSerializer() => new MessagePackSerializer();
        }

        protected SerializerTestBase()
        {
            Serializer = CreateSerializer();
        }

        protected abstract ISerializer CreateSerializer();

        public ISerializer Serializer { get; }

        [Fact]
        public Task Write_NullStream_ThrowsArgumentNullException()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(null, null));
        }

        [Fact]
        public async Task Write_NullObject_Throws()
        {
            // arrange
            MemoryStream stream = new MemoryStream();
            await Assert.ThrowsAsync<ArgumentNullException>(() => Serializer.WriteAsync(stream, null));
        }

        [Fact]
        public async Task WriteRead_Derived_EnsureDeserializedProperly()
        {
            // arrange
            A obj = new C(3);
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);

            // act 
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(A));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_ComplexType_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);

            // act 
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(CompositeType));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_SpecificType_EnsureDeserializedProperly()
        {
            // arrange
            SimpleCustomType obj = new SimpleCustomType { BoolProperty = false };
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);

            // act
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(SimpleCustomType));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task WriteRead_ComplexTypeAndSameStream_EnsureDeserializedProperly()
        {
            // arrange
            CompositeType obj = CompositeType.CreateRandom();
            MemoryStream stream = new MemoryStream();
            await Serializer.WriteAsync(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);

            // act 
            var result = await Serializer.ReadAsync(stream, typeof(CompositeType));

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
            await Serializer.WriteAsync(stream, obj);
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(int));

            // assert
            Assert.Equal(obj, result);
        }

        [Fact]
        public async Task ReadWrite_SimpleList_EnsureValidResult()
        {
            // arrange
            List<int> obj = new List<int>() { 1, 2, 3 };
            MemoryStream stream = new MemoryStream();

            // act
            await Serializer.WriteAsync(stream, obj);
            var result = await Serializer.ReadAsync(new MemoryStream(stream.ToArray()), typeof(List<int>));

            // assert
            Assert.Equal(obj, result);
        }

        [InlineData(11.1)]
        [InlineData("value")]
        [InlineData(10)]
        [InlineData(true)]
        [Theory]
        public Task WriteSimpleParameter_EnsureDeserializedProperly(object parameter)
        {
            return WriteSimpleParameterHelper(parameter, parameter);
        }

        [MemberData(nameof(GetManyProperties), 1)]
        [MemberData(nameof(GetManyProperties), 2)]
        [MemberData(nameof(GetManyProperties), 10)]
        [Theory]
        public async Task WriteSimpleParameters_EnsureDeserializedProperly((ParameterMetadata metadata, object value)[] parameters)
        {
            MemoryStream output = new MemoryStream();
            var values = parameters.Select(p => p.value).ToArray();
            var metadata = parameters.Select(p => p.metadata).ToArray();

            await Serializer.WriteParametersAsync(output, metadata, values);

            output.Seek(0, SeekOrigin.Begin);
            var outputValues = await Serializer.ReadParametersAsync(output, metadata);

            for (int i = 0; i < parameters.Length; i++)
            {
                Assert.Equal(parameters[i].value, outputValues[i]);
            }
        }

        [Fact]
        public Task WriteCancellationTokenParameter_ShouldBeNull()
        {
            return WriteSimpleParameterHelper(CancellationToken.None, null);
        }

        [Fact]
        public Task WriteParamterAsIntArray()
        {
            return WriteSimpleParameterHelper(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 });
        }

        [Fact]
        public Task WriteParamterAsShortArray()
        {
            return WriteSimpleParameterHelper(new short[] { 1, 2, 3 }, new short[] { 1, 2, 3 });
        }

        [Fact]
        public Task WriteParamterAsDoubleArray()
        {
            return WriteSimpleParameterHelper(new double[] { 1, 2, 3 }, new double[] { 1, 2, 3 });
        }

        [Fact]
        public Task ComplexParameters_OK()
        {
            return WriteReadParameters(CompositeType.CreateRandom(), DateTime.UtcNow, 10, false, 11.1, new string[] { "a", "b" }, new int[] { 2, 3, 3 });
        }

        [Fact]
        public Task DerivedParameter_EnsureTypePreserved()
        {
            return WriteReadParameters(new A(1), new B(2), new C(3), new C(1));
        }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        public class A
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            public A(int id)
            {
                Id = id;
            }

            public int Id { get; }

            public override bool Equals(object obj)
            {
                return GetType() == obj?.GetType() && (obj as A).Id == Id;
            }
        }

        public class B : A
        {
            public B(int id) : base(id)
            {
            }
        }

        public class C : B
        {
            public C(int id) : base(id)
            {
            }
        }

        private async Task WriteReadParameters(params object[] parameters)
        {
            MemoryStream output = new MemoryStream();

            var metadata = parameters.Select(v => new ParameterMetadata(v.GetType(), v.GetType().Name)).ToArray();
            await Serializer.WriteParametersAsync(output, metadata, parameters);

            output.Seek(0, SeekOrigin.Begin);
            var outputValues = await Serializer.ReadParametersAsync(output, metadata);

            var json = global::MessagePack.MessagePackSerializer.ToJson(output.ToArray());

            for (int i = 0; i < parameters.Length; i++)
            {
                Assert.Equal(parameters[i], outputValues[i]);
            }
        }

        private async Task WriteSimpleParameterHelper(object parameter, object expected)
        {
            MemoryStream output = new MemoryStream();
            var parameters = new[] { new ParameterMetadata(parameter.GetType(), "name") };
            var values = new object[] { parameter };

            await Serializer.WriteParametersAsync(output, parameters, values);

            output.Seek(0, SeekOrigin.Begin);
            var outputValues = await Serializer.ReadParametersAsync(output, parameters);

            Assert.Equal(expected, outputValues[0]);
        }


        public static IEnumerable<object[]> GetManyProperties(int parameters)
        {
            yield return new object[] { Enumerable.Range(0, parameters).Select(index => (new ParameterMetadata(typeof(int), $"name_{index}"), (object)index)).Cast<object>().ToArray() };

            yield return new object[] { Enumerable.Range(0, parameters).Select(index => (new ParameterMetadata(typeof(CompositeType), $"name_{index}"), (object)CompositeType.CreateRandom())).Cast<object>().ToArray() };
        }
    }
}
