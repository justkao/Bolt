using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Generators
{
    public class TypeFormatter
    {
        private readonly List<string> _namespaces = new List<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text",
            "System",
            "System.Threading.Tasks",
            "System.IO",
        };

        private readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>()
        {
            {typeof (string), "string"},
            {typeof (bool), "bool"},
            {typeof (int), "int"},
            {typeof (short), "short"},
            {typeof (long), "long"},
            {typeof (double), "double"},
        };

        public virtual string FormatType(Type type)
        {
            return DoFormat(type);
        }

        public virtual void AddNamespace(string ns)
        {
            if (!_namespaces.Contains(ns))
            {
                _namespaces.Add(ns);
            }
        }

        public virtual IEnumerable<string> GetNamespaces()
        {
            return _namespaces.Distinct().OrderBy(n => n).Distinct().ToList();
        }

        private string DoFormat(Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                string name = type.Name.Substring(0, type.Name.IndexOf('`'));

                if (!_namespaces.Contains(type.Namespace))
                {
                    name = type.Namespace + "." + name;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(name + "<");

                foreach (Type argument in type.GenericTypeArguments)
                {
                    sb.Append(DoFormat(argument) + ", ");
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append(">");
                return sb.ToString();
            }

            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }

            if (_namespaces.Contains(type.Namespace))
            {
                if (typeof(Attribute).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    return type.Name.Substring(0, type.Name.IndexOf("Attribute", StringComparison.Ordinal));
                }

                return type.Name;
            }

            if (typeof(Attribute).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return type.FullName.Substring(0, type.FullName.IndexOf("Attribute", StringComparison.Ordinal));
            }

            return type.FullName;
        }
    }
}
