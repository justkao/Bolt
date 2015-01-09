using System.Runtime.Serialization;

namespace Bolt.Core.Serialization.Test
{
    [DataContract]
    public class SimpleCustomType
    {
        [DataMember(Order = 1)]
        public bool BoolProperty { get; set; }

        protected bool Equals(SimpleCustomType other)
        {
            return BoolProperty.Equals(other.BoolProperty);
        }

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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((SimpleCustomType)obj);
        }

        public override int GetHashCode()
        {
            return BoolProperty.GetHashCode();
        }
    }
}