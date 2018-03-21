using Bolt.Serialization;
using Bolt.Serialization.MessagePack;

namespace Bolt.Server.IntegrationTest
{
    public class MessagePackSerializationTest : SerializationTest
    {
        protected override ISerializer CreateSerializer() => new MessagePackSerializer();
    }
}
