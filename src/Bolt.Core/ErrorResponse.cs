using System.Runtime.Serialization;

namespace Bolt
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Order = 1)]
        public byte[] RawException { get; set; }
    }
}
