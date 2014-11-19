using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Bolt
{
    public abstract class ContractDescriptor : IEnumerable<ActionDescriptor>
    {
        protected ContractDescriptor(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }

        private readonly List<ActionDescriptor> _actions = new List<ActionDescriptor>();

        protected ActionDescriptor Add(string action, Type parameters, MethodInfo method)
        {
            ActionDescriptor descriptor = new ActionDescriptor(action, parameters, this, method);
            _actions.Add(descriptor);
            return descriptor;
        }

        public IEnumerator<ActionDescriptor> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}