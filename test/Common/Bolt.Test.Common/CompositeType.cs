using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Bolt.Test.Common
{
    [DataContract]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class CompositeType
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public static CompositeType CreateRandom(bool createInner = true)
        {
            Random rnd = new Random();
            CompositeType type = new CompositeType();
            type.Int = rnd.Next();
            type.Double = rnd.NextDouble();
            type.Bool = rnd.Next(0, 10) % 2 == 0;
            type.DateTime = DateTime.UtcNow;
            type.List = new List<string>();

            for (int i = 0; i < rnd.Next(0, 20); i++)
            {
                type.List.Add(rnd.Next().ToString());
            }

            if (createInner)
            {
                type.Inner = new List<CompositeType>();

                for (int i = 0; i < rnd.Next(0, 20); i++)
                {
                    type.Inner.Add(CreateRandom(false));
                }
            }

            return type;
        }

        [DataMember(Order = 1)]
        public int Int { get; set; }

        [DataMember(Order = 2)]
        public double Double { get; set; }

        [DataMember(Order = 3)]
        public List<string> List { get; set; }

        [DataMember(Order = 4)]
        public DateTime DateTime { get; set; }

        [DataMember(Order = 5)]
        public bool Bool { get; set; }

        [DataMember(Order = 6)]
        public List<CompositeType> Inner { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CompositeType)obj);
        }

        protected bool Equals(CompositeType other)
        {
            if (!List.EmptyIfNull().SequenceEqual(other.List.EmptyIfNull()))
            {
                return false;
            }

            if (!Inner.EmptyIfNull().SequenceEqual(other.Inner.EmptyIfNull()))
            {
                return false;
            }

            return Int == other.Int && Double == other.Double && DateTime.Equals(other.DateTime) && Bool.Equals(other.Bool);
        }
    }
}