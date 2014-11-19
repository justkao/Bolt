using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public class ContractDescriptorGenerator : ContractGeneratorBase
    {
        public ContractDescriptorGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ContractDescriptorGenerator(StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(output, formatter, provider)
        {
        }

        public string BaseClass { get; set; }

        public override void Generate()
        {
            AddUsings("System.Reflection");

            IEnumerable<MethodInfo> methods = ContractDefinition.GetEffectiveMethods().ToList();
            TypeDescriptor descriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);

            BeginNamespace(descriptor.Namespace);

            WriteLine("public partial class {0} : {1}", descriptor.Name, BaseClass ?? FormatType<ContractDescriptor>());
            BeginBlock();

            WriteLine("public {0}() : base(typeof({1}))", descriptor.Name, ContractDefinition.Root.FullName);
            BeginBlock();

            foreach (MethodInfo method in methods)
            {
                MethodDescriptor methodDescriptor = MetadataProvider.GetMethodDescriptor(ContractDefinition, method);
                string parametersType = HasParameters(method)
                                            ? MetadataProvider.GetParameterDescriptor(method.DeclaringType, method).FullName
                                            : typeof(Empty).FullName;

                WriteLine(
                    "{0} = Add(\"{0}\", typeof({1}), typeof({2}).GetTypeInfo().GetMethod(\"{3}\"));",
                    methodDescriptor.Name,
                    parametersType,
                    FormatType(method.DeclaringType),
                    method.Name);
            }

            EndBlock();

            WriteLine();

            WriteLine("public static readonly {0} Default = new {0}();", descriptor.Name, descriptor);
            WriteLine();

            foreach (MethodInfo method in methods)
            {
                MethodDescriptor methodDescriptor = MetadataProvider.GetMethodDescriptor(ContractDefinition, method);
                WriteLine("public virtual {0} {1} {{ get; private set; }}", FormatType<ActionDescriptor>(), methodDescriptor.Name);

                if (!Equals(method, methods.Last()))
                {
                    WriteLine();
                }
            }

            EndBlock();

            EndNamespace();
            WriteLine();
        }
    }
}