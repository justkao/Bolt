namespace Bolt.Service.Test
{
    public abstract class SerializerTestBase
    {
        protected SerializerTestBase()
        {
            Serializer = CreateSerializer();
        }

        public ISerializer Serializer { get; private set; }

        protected abstract ISerializer CreateSerializer();
    }
}