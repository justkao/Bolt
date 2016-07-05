using System;
using System.Runtime.Serialization;

namespace Bolt.Performance.Core.Contracts
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
            return new Person
            {
                Name = "Name" + id,
                Address = new Address { City = "Address" + id },
                BornDate = DateTime.UtcNow,
                Id = id,
                Surname = "Surname" + id
            };
        }
    }
}
