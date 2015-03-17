using System;
using System.Runtime.Serialization;

namespace TestService.Core
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

        [DataMember(Order = 4)]
        public DateTime BornDate { get; set; }

        [DataMember(Order = 5, IsRequired = false)]
        public Address Address { get; set; }

        public static Person Create(int id)
        {
            return new Person()
            {
                Name = "Name" + id,
                Address = new Address() { City = "Address" + id },
                BornDate = DateTime.UtcNow,
                Id = id,
                Surname = "Surname" + id
            };
        }
    }

    [DataContract]
    public enum AddresType
    {
        [EnumMember]
        Local,
        [EnumMember]
        Remote
    }

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
