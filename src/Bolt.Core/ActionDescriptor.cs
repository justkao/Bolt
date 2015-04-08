using System;
using System.Reflection;

namespace Bolt
{
    /// <summary>
    /// Descriptor of single Bolt action.
    /// </summary>
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

        public string Name { get; }

        /// <summary>
        /// The parameters object used to hold all the data the action requires to be executed on server.
        /// </summary>
        public Type Parameters { get; private set; }

        /// <summary>
        /// Owner of this action descriptor.
        /// </summary>
        public ContractDescriptor Contract { get; }

        /// <summary>
        /// The method this descriptor refers to.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Determines whether the action has any parameters.
        /// </summary>
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ActionDescriptor)obj);
        }

        public override int GetHashCode()
        {
            return _id?.GetHashCode() ?? 0;
        }

        public static bool operator ==(ActionDescriptor left, ActionDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActionDescriptor left, ActionDescriptor right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{Contract.Name}/{Name}";
        }
    }
}