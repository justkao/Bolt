using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Generators
{
    public class ClassDescriptor
    {
        public static ClassDescriptor Create<T>()
        {
            return new ClassDescriptor(typeof(T));
        }

        public ClassDescriptor()
        {
            BaseClasses = new string[0];
        }

        public ClassDescriptor(Type type)
            : this(type.Name, type.Namespace)
        {
        }

        public ClassDescriptor(string name, string @namespace)
            : this(name, @namespace, new string[0])
        {
        }

        public ClassDescriptor(string name, string @namespace, params string[] baseClasses)
        {
            Name = name;
            Namespace = @namespace;
            BaseClasses = baseClasses.ToList();
        }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0}.{1}", Namespace, Name);
            }
        }

        public IEnumerable<string> BaseClasses { get; set; }

        public bool IsInterface { get; set; }
    }
}