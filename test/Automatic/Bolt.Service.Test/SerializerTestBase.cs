using Bolt.Core.Serialization;
using NUnit.Framework;
using System;

namespace Bolt.Service.Test
{
    [TestFixture(SerializerType.Json)]
    [TestFixture(SerializerType.Proto)]
    [TestFixture(SerializerType.Xml)]
    [TestFixture(SerializerType.Protected)]
    public abstract class SerializerTestBase
    {
        private readonly SerializerType _serializerType;

        protected SerializerTestBase(SerializerType serializerType)
        {
            _serializerType = serializerType;
        }


        public ISerializer Serializer { get; private set; }

        [TestFixtureSetUp]
        protected virtual void Init()
        {
            ISerializer serializer;

            switch (_serializerType)
            {
                case SerializerType.Proto:
                    serializer = new ProtocolBufferSerializer();
                    break;
                case SerializerType.Json:
                    serializer = new JsonSerializer();
                    break;
                case SerializerType.Xml:
                    serializer = new XmlSerializer();
                    break;
                case SerializerType.Protected:
                    serializer = new ProtectedSerializer(new JsonSerializer(), "assssssssssssssssssssssssssfasdfsfsdf");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Serializer = serializer;
        }
    }
}