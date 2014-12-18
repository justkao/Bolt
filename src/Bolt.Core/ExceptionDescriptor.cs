using System.Runtime.Serialization;

namespace Bolt
{
    [DataContract]
    public class ExceptionDescriptor
    {
        [DataMember(Order = 1 )]
        public string Message { get; set; }

        [DataMember(Order = 2)]
        public string Type { get; set; }

        [DataMember(Order = 3)]
        public string StackTrace { get; set; }
    }
}