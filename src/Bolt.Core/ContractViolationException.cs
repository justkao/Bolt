using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bolt
{
    [Serializable]
    public class ContractViolationException : BoltException
    {
        public ContractViolationException(string message, Type contract, MethodInfo action) : base(message)
        {
        }

        public ContractViolationException(string message, Type contract) : base(message)
        {
        }

        public ContractViolationException(Type contract, MethodInfo action) : base($"Action '{action.Name}' on contract '{contract.Name}' violates the Bolt contract rules.")
        {
        }

        public ContractViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
