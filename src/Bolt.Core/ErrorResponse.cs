using System.Runtime.Serialization;

namespace Bolt
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Order = 1)]
        public string JsonException { get; set; }
    }
}
