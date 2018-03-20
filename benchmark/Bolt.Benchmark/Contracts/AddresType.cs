using System.Runtime.Serialization;

namespace Bolt.Benchmark.Contracts
{
    [DataContract]
    public enum AddresType
    {
        [EnumMember]
        Local,

        [EnumMember]
        Remote
    }
}