using Bolt.Serialization;

namespace Bolt.Server.IntegrationTest
{
    public class JsonSerializationTest : SerializationTest
    {
        protected override ISerializer CreateSerializer() => new JsonSerializer();
    }
}
