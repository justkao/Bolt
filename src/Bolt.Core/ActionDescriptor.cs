using System;
using System.Reflection;

namespace Bolt
{
    public class ActionDescriptor
    {
        private readonly string _id;

        public ActionDescriptor(string name, Type parameters, ContractDescriptor contract, MethodInfo method)
        {
            Name = name;
            Parameters = parameters;
            Contract = contract;
            Method = method;
            HasParameters = parameters != typeof(Empty);
            _id = method.DeclaringType.Name + name;
        }

        public string Name { get; private set; }

        public Type Parameters { get; private set; }

        public ContractDescriptor Contract { get; private set; }

        public MethodInfo Method { get; private set; }

        public bool HasParameters { get; private set; }

        protected bool Equals(ActionDescriptor other)
        {
            return string.Equals(_id, other._id);
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
            return Equals((ActionDescriptor)obj);
        }

        public override int GetHashCode()
        {
            return _id != null ? _id.GetHashCode() : 0;
        }

        public static bool operator ==(ActionDescriptor left, ActionDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActionDescriptor left, ActionDescriptor right)
        {
            return !Equals(left, right);
        }
    }
}