using System.Runtime.Serialization;
using Bolt.Client.Channels;
using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public class ConfigureSessionTest
    {
        [Fact]
        public void Write_Ok()
        {
            SessionChannel channel = new SessionChannel(typeof (ITestContract),
                Mock.Of<IServerProvider>(), new ClientConfiguration());

            channel.ConfigureSession(ctxt =>
            {
                ctxt.Write("val", 10);
                Assert.Equal(10, ctxt.Read<int>("val"));
            });

            Assert.NotNull(channel.InitSessionParameters);
            Assert.True(channel.InitSessionParameters.UserData.ContainsKey("val"));
        }

        [Fact]
        public void Write_ComplexObject_Ok()
        {
            SessionChannel channel = new SessionChannel(typeof(ITestContract),
                Mock.Of<IServerProvider>(), new ClientConfiguration());

            channel.ConfigureSession(ctxt =>
            {
                Person person = new Person() {Name = "SomeName", Surname = "SomeUserName"};
                ctxt.Write("val", person);
                var deserialized = ctxt.Read<Person>("val");
                Assert.NotNull(deserialized);
                Assert.Equal(person.Name, deserialized.Name);
                Assert.Equal(person.Surname, deserialized.Surname);
            });

            Assert.NotNull(channel.InitSessionParameters);
            Assert.True(channel.InitSessionParameters.UserData.ContainsKey("val"));
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