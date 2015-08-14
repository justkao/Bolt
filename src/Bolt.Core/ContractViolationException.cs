using System;
using System.Reflection;

namespace Bolt
{
    public class ContractViolationException : BoltException
    {
        public ContractViolationException(string message, Type contract, MethodInfo action) : base(message)
        {
            Contract = contract;
            Action = action;
        }

        public ContractViolationException(string message, Type contract) : base(message)
        {
            Contract = contract;
        }

        public ContractViolationException(Type contract, MethodInfo action) : base($"Action '{action.Name}' is violation Bolt contract.")
        {
            Contract = contract;
            Action = action;
        }

        public Type Contract { get; }

        public MethodInfo Action { get;  }
    }
}
