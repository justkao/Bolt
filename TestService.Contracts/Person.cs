using System.Runtime.Serialization;

namespace TestService.Contracts
{
    [DataContract]
    public class Person
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public string Surname { get; set; }
    }
}
