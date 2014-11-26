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
            StaticAccessorProperty = "Default";
            Modifier = "public";
        }

        public string StaticAccessorProperty { get; set; }

        public string Modifier { get; set; }

        public override void Generate()
        {
            ClassDescriptor descriptor = ContractDescriptor;
            AddUsings(descriptor.Namespace);
            AddUsings("System.Runtime");
            AddUsings("System.Reflection");

            List<MethodInfo> methods = ContractDefinition.GetEffectiveMethods().ToList();

            ClassGenerator classGenerator = CreateClassGenerator(descriptor);
            classGenerator.Modifier = Modifier;

            classGenerator.GenerateClass(
                g =>
                {
                    WriteLine("public {0}() : base(typeof({1}), \"{2}\")", descriptor.Name, ContractDefinition.Root.FullName, ContractDefinition.Name);
                    using (WithBlock())
                    {
                        foreach (MethodInfo method in methods)
                        {
                            MethodDescriptor methodDescriptor = MetadataProvider.GetMethodDescriptor(ContractDefinition, method);

                            string parametersType = methodDescriptor.Parameters.FullName;

                            WriteLine(
                                "{0} = Add(\"{0}\", typeof({1}), typeof({2}).GetTypeInfo().GetMethod(\"{3}\"));",
                                methodDescriptor.Name,
                                parametersType,
                                FormatType(method.DeclaringType),
                                method.Name);
                        }
                    }

                    WriteLine();
                    WriteLine("public static readonly {0} {1} = new {0}();", descriptor.Name, StaticAccessorProperty);
                    WriteLine();

                    foreach (MethodInfo method in methods)
                    {
                        MethodDescriptor methodDescriptor = MetadataProvider.GetMethodDescriptor(ContractDefinition, method);
                        g.WritePublicReadonlyProperty(FormatType<ActionDescriptor>(), methodDescriptor.Name);
                        if (!Equals(method, methods.Last()))
                        {
                            WriteLine();
                        }
                    }
                });
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            ClassDescriptor descriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);
            descriptor.BaseClasses = new[] { FormatType<ContractDescriptor>() };
            return descriptor;
        }
    }
}