using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Dnx.Runtime.Loader;
using Xunit;

namespace Bolt.Core.Test
{
    public class ObjectSerializerTest
    {
        public ObjectSerializerTest()
        {
            Serializer = new JsonSerializer();
        }

        public ISerializer Serializer { get; set; }

        [Fact]
        public void WriteSerializer_TryRead_Throws()
        {
            var objectSerializer = Serializer.CreateSerializer();
            Assert.Throws<InvalidOperationException>(() =>
            {
                object val;
                objectSerializer.TryRead("test", typeof (string), out val);
            });
        }

        [Fact]
        public void ReadSerializer_TryWrite_Throws()
        {
            var objectSerializer = Serializer.CreateSerializer(new MemoryStream());
            Assert.Throws<InvalidOperationException>(() =>
            {
                objectSerializer.Write("test", typeof (string), "val");
            });
        }

        [Fact]
        public void WriteString_Ok()
        {
            var objectSerializer = Serializer.CreateSerializer();

            objectSerializer.Write("testProperty", typeof (string), "stringValue");

            string output = ReadString(objectSerializer.GetOutputStream());

            Assert.Contains("testProperty", output);
            Assert.Contains("stringValue", output);
        }

        [Fact]
        public void Initial_ShouldBeEmpty()
        {
            var objectSerializer = Serializer.CreateSerializer();
            Assert.True(objectSerializer.IsEmpty);
        }

        [Fact]
        public void WriteNull_ShouldBeEmpty()
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof (string), null);

            Assert.True(objectSerializer.IsEmpty);
        }

        [Fact]
        public void Write_ShouldNotBeEmpty()
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof(string), "value");

            Assert.False(objectSerializer.IsEmpty);
        }

        [Fact]
        public void WriteObject_Ok()
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof (SimpleObject),
                new SimpleObject() {Name = "simpleObjectName", Description = "SimpleObjectDescription"});

            string output = ReadString(objectSerializer.GetOutputStream());

            Assert.Contains("testProperty", output);
            Assert.Contains("simpleObjectName", output);
            Assert.Contains("SimpleObjectDescription", output);
        }

        [Fact]
        public void WriteMultipleObjects_Ok()
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty1", typeof(SimpleObject),
                new SimpleObject() { Name = "simpleObjectName1", Description = "SimpleObjectDescription1" });
            objectSerializer.Write("testProperty2", typeof (SimpleObject),
                new SimpleObject() {Name = "simpleObjectName2", Description = "SimpleObjectDescription2"});


            string output = ReadString(objectSerializer.GetOutputStream());

            Assert.Contains("testProperty1", output);
            Assert.Contains("simpleObjectName1", output);
            Assert.Contains("SimpleObjectDescription1", output);
            Assert.Contains("testProperty2", output);
            Assert.Contains("simpleObjectName2", output);
            Assert.Contains("SimpleObjectDescription2", output);
        }

        [InlineData("")]
        [InlineData(null)]
        [InlineData("some string value")]
        [Theory]
        public void ReadString_Ok(string serializedValue)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof (string), serializedValue);
            var output = objectSerializer.GetOutputStream();

            var deserialzier = Serializer.CreateSerializer(output);

            object val;
            deserialzier.TryRead("testProperty", typeof (string), out val);
            Assert.Equal(serializedValue, val);
        }

        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        [Theory]
        public void ReadInt_Ok(int serializedValue)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof(int), serializedValue);
            var output = objectSerializer.GetOutputStream();

            var deserialzier = Serializer.CreateSerializer(output);

            object val;
            deserialzier.TryRead("testProperty", typeof(int), out val);
            Assert.Equal(serializedValue, val);
        }

        [InlineData(0, 1)]
        [InlineData(2, 3)]
        [InlineData(4, 5)]
        [Theory]
        public void ReadMultiple_Ok(int first, int second)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("prop1", typeof (int), first);
            objectSerializer.Write("prop2", typeof(int), second);

            var deserializer = Serializer.CreateSerializer(objectSerializer.GetOutputStream());

            object val;
            deserializer.TryRead("prop1", typeof (int), out val);
            Assert.Equal(first, val);
            deserializer.TryRead("prop2", typeof(int), out val);
            Assert.Equal(second, val);
        }

        [InlineData(0, 1)]
        [InlineData(2, 3)]
        [InlineData(4, 5)]
        [Theory]
        public void ReadMultipleNotInOrder_Ok(int first, int second)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("prop1", typeof(int), first);
            objectSerializer.Write("prop2", typeof(int), second);

            var deserializer = Serializer.CreateSerializer(objectSerializer.GetOutputStream());

            object val;
            deserializer.TryRead("prop2", typeof(int), out val);
            Assert.Equal(second, val);
            deserializer.TryRead("prop1", typeof(int), out val);
            Assert.Equal(first, val);
        }

        [InlineData(0, "test", true)]
        [InlineData("test", "test2", 1)]
        [InlineData(1, "test2", 5.0)]
        [Theory]
        public void ReadMultipleInOrder_Ok(object first, object second, object third)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("prop1", first.GetType(), first);
            objectSerializer.Write("prop2", second.GetType(), second);
            objectSerializer.Write("prop3", third.GetType(), third);

            var deserializer = Serializer.CreateSerializer(objectSerializer.GetOutputStream());

            object val;
            deserializer.TryRead("prop1", first.GetType(), out val);
            Assert.Equal(first, val);

            deserializer.TryRead("prop2", second.GetType(), out val);
            Assert.Equal(second, val);

            deserializer.TryRead("prop3", third.GetType(), out val);
            Assert.Equal(third, val);
        }

        [InlineData(0, "test", true)]
        [InlineData("test", "test2", 1)]
        [InlineData(1, "test2", 5.0)]
        [Theory]
        public void ReadMultipleNotInOrder_Ok(object first, object second, object third)
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("prop1", first.GetType(), first);
            objectSerializer.Write("prop2", second.GetType(), second);
            objectSerializer.Write("prop3", third.GetType(), third);

            var deserializer = Serializer.CreateSerializer(objectSerializer.GetOutputStream());

            object val;
            deserializer.TryRead("prop2", second.GetType(), out val);
            Assert.Equal(second, val);

            deserializer.TryRead("prop1", first.GetType(), out val);
            Assert.Equal(first, val);

            deserializer.TryRead("prop3", third.GetType(), out val);
            Assert.Equal(third, val);
        }

        [InlineData(0, null, true)]
        [InlineData("test", null, 1)]
        [InlineData(1, null, 5.0)]
        [Theory]
        public void ReadMultipleWithObjectsNotInOrder_Ok(object first, object second, object third)
        {
            second = new SimpleObject() {Name = "testName", Description = "testDescription"};

            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("prop1", first.GetType(), first);
            objectSerializer.Write("prop2", second.GetType(), second);
            objectSerializer.Write("prop3", third.GetType(), third);

            var deserializer = Serializer.CreateSerializer(objectSerializer.GetOutputStream());

            object val;
            deserializer.TryRead("prop2", second.GetType(), out val);
            var obj = Assert.IsType<SimpleObject>(val);
            Assert.Equal((second as SimpleObject).Name, obj.Name);
            Assert.Equal((second as SimpleObject).Description, obj.Description);

            deserializer.TryRead("prop1", first.GetType(), out val);
            Assert.Equal(first, val);

            deserializer.TryRead("prop3", third.GetType(), out val);
            Assert.Equal(third, val);
        }

        [Fact]
        public void ReadObject_Ok()
        {
            var objectSerializer = Serializer.CreateSerializer();
            objectSerializer.Write("testProperty", typeof (SimpleObject),
                new SimpleObject() {Name = "objname", Description = "objDescription"});
            var output = objectSerializer.GetOutputStream();

            var deserialzier = Serializer.CreateSerializer(output);

            object val;
            deserialzier.TryRead("testProperty", typeof(SimpleObject), out val);
            Assert.NotNull(val);
            var person = Assert.IsType<SimpleObject>(val);

            Assert.Equal("objname", person.Name);
            Assert.Equal("objDescription", person.Description);
        }


        private string ReadString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public class SimpleObject
        {
            public string Name { get; set; }

            public string Description { get; set; }
        }
    }
}
