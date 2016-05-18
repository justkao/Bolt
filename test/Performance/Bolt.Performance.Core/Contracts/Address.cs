using System.Runtime.Serialization;

namespace Bolt.Performance.Core.Contracts
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