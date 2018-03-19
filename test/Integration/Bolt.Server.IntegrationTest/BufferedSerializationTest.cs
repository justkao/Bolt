using Bolt.Serialization;

namespace Bolt.Server.IntegrationTest
{
    public class BufferedSerializationTest : SerializationTest
    {
        protected override ISerializer CreateSerializer() => new BufferedSerializer(new JsonSerializer());
    }
}
