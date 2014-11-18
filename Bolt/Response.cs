using System.Runtime.Serialization;

namespace Bolt
{
    [DataContract]
    internal class Response<T>
    {
        [DataMember(Order = 1)]
        public T Data { get; set; }
    }
}