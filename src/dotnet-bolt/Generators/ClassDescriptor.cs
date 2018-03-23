using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Tools.Generators
{
    public class ClassDescriptor
    {
        public ClassDescriptor()
        {
            BaseClasses = Array.Empty<string>();
        }

        public ClassDescriptor(Type type)
            : this(type.Name, type.Namespace)
        {
        }

        public ClassDescriptor(string name, string @namespace)
            : this(name, @namespace, Array.Empty<string>())
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

        public string FullName => $"{Namespace}.{Name}";

        public IEnumerable<string> BaseClasses { get; set; }

        public bool IsInterface { get; set; }

        public static ClassDescriptor Create<T>()
        {
            return new ClassDescriptor(typeof(T));
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}