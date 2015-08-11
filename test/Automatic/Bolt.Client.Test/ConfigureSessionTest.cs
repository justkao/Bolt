using System.Runtime.Serialization;

using Bolt.Client.Channels;
using Bolt.Client.Pipeline;
using Bolt.Session;

using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public class ConfigureSessionTest
    {
        [Fact]
        public void Write_Ok()
        {
            ConfigureSessionContext ctxt = CreateContext();

            ctxt.Write("val", 10);
            Assert.Equal(10, ctxt.Read<int>("val"));
            Assert.NotNull(ctxt.Parameters);
            Assert.True(ctxt.Parameters.UserData.ContainsKey("val"));
        }

        [Fact]
        public void Write_ComplexObject_Ok()
        {
            ConfigureSessionContext ctxt = CreateContext();

            Person person = new Person() { Name = "SomeName", Surname = "SomeUserName" };
            ctxt.Write("val", person);
            var deserialized = ctxt.Read<Person>("val");
            Assert.NotNull(deserialized);
            Assert.Equal(person.Name, deserialized.Name);
            Assert.Equal(person.Surname, deserialized.Surname);

            Assert.NotNull(ctxt.Parameters);
            Assert.True(ctxt.Parameters.UserData.ContainsKey("val"));
        }

        private static ConfigureSessionContext CreateContext()
        {
            return new ConfigureSessionContext(new JsonSerializer(), new InitSessionParameters());
        }

        [DataContract]
        private class Person
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Surname { get; set; }
        }
    }
}