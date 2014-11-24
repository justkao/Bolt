using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt
{
    public abstract class ContractDescriptor : IEnumerable<ActionDescriptor>
    {
        protected ContractDescriptor(Type type, string name)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Type = type;
            Name = name;
        }

        public Type Type { get; private set; }

        public string Name { get; private set; }

        public ActionDescriptor Find(MethodInfo info)
        {
            return this.FirstOrDefault(v => Equals(v.Method, info));
        }

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