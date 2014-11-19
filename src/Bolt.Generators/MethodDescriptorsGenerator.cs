using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Bolt.Generators
{
    public class TypeDescriptorGenerator : ContractGeneratorBase
    {
        public TypeDescriptorGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public TypeDescriptorGenerator(StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(output, formatter, provider)
        {
        }

        public string BaseClass { get; set; }

        public override void Generate()
        {
            IReadOnlyCollection<Type> contracts = Contract.GetEffectiveContracts();

            foreach (Type type in contracts)
            {
                TypeDescriptor descriptor = MetadataProvider.GetTypeDescriptor(type);

                BeginNamespace(descriptor.Namespace);

                WriteLine("public static class {0}", descriptor.Name);
                BeginBlock();

                foreach (MethodInfo info in Contract.GetEffectiveMethods(type))
                {
                    MethodDescriptor methodDescriptor = MetadataProvider.GetMethodDescriptor(Contract, info);

                    WriteLine("public static readonly {0} {1} = new {2};", FormatType<MethodDescriptor>(), methodDescriptor.Method, MethodDescriptorConstructor(methodDescriptor, info));
                    WriteLine();
                }

                EndBlock();

                EndNamespace();
                WriteLine();
            }
        }

        protected virtual string MethodDescriptorConstructor(MethodDescriptor descriptor, MethodInfo info)
        {
            return string.Format("{3}(\"{0}\",\"{1}\",\"{2}\", typeof({4}))", descriptor.Contract, descriptor.Method,
                descriptor.Url, FormatType<MethodDescriptor>(),
                HasParameters(info)
                    ? MetadataProvider.GetParametersClass(info.DeclaringType, info).FullName
                    : typeof(Empty).FullName);
        }
    }
}