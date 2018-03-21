using System.Runtime.Serialization;

namespace Bolt.Test.Common
{
    [DataContract]
    public class SimpleCustomType
    {
        [DataMember(Order = 1)]
        public bool BoolProperty { get; set; }

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
            return Equals((SimpleCustomType)obj);
        }

        public override int GetHashCode()
        {
            return BoolProperty.GetHashCode();
        }

        protected bool Equals(SimpleCustomType other)
        {
            return BoolProperty.Equals(other.BoolProperty);
        }
    }
}