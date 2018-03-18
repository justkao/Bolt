using System.Runtime.Serialization;

namespace Bolt.Benchmark.Contracts
{
    [DataContract]
    public class Address
    {
        [DataMember(Order = 1)]
        public string City { get; set; }

        [DataMember(Order = 2)]
        public string State { get; set; }

        [DataMember(Order = 3)]
        public AddresType Type { get; set; }
    }
}