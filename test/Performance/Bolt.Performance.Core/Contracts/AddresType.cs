using System.Runtime.Serialization;

namespace Bolt.Performance.Contracts
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