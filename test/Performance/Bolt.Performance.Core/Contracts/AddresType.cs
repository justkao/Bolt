using System.Runtime.Serialization;

namespace Bolt.Performance.Core.Contracts
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