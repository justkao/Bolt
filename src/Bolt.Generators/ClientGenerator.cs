using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Client;

namespace Bolt.Generators
{
    public class ClientGenerator : ContractGeneratorBase
    {
        private string _contractDescriptorProperty;

        public ClientGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ClientGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public virtual string ContractDescriptorProperty
        {
            get
            {
                if (_contractDescriptorProperty == null)
                {
                    return string.Format("{0}Descriptor", ContractDefinition.Name);

                }

                return _contractDescriptorProperty;
            }

            set { _contractDescriptorProperty = value; }
        }

        public bool ForceAsync { get; set; }

        public bool GenerateFactory { get; set; }

        public override void Generate()
        {
            ClassDescriptor contractDescriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);
            ClassGenerator generator = CreateClassGenerator(ContractDescriptor);

            generator.GenerateClass(
                g =>
                {
                    WriteLine("public {0} {1} {{ get; set; }}", contractDescriptor.FullName, ContractDescriptorProperty);
                    WriteLine();

                    List<Type> contracts = ContractDefinition.GetEffectiveContracts().ToList();

                    foreach (Type type in contracts)
                    {
                        GenerateMethods(g, type);
                    }
                });
            WriteLine();

            if (GenerateFactory)
            {
                ClassDescriptor descriptor = new ClassDescriptor(
                    generator.Descriptor.Name + "Factory",
                    generator.Descriptor.Namespace,
                    string.Format("Bolt.Client.ChannelFactory<{0}, {1}>", generator.Descriptor.Name, contractDescriptor.Name));

                ClientFactoryGenerator clientFactoryGenerator = new ClientFactoryGenerator(Output, Formatter, IntendProvider)
                                                                    {
                                                                        ContractDefinition =
                                                                            ContractDefinition,
                                                                        ContractDescriptor =
                                                                            descriptor
                                                                    };

                clientFactoryGenerator.Generate();
                WriteLine();
            }
        }

        private void GenerateMethods(ClassGenerator classGenerator, Type contract)
        {
            MethodInfo[] methods = ContractDefinition.GetEffectiveMethods(contract).ToArray();

            foreach (MethodInfo method in methods)
            {
                MethodDescriptor descriptor = MetadataProvider.GetMethodDescriptor(ContractDefinition, method);

                GenerateMethod(classGenerator, descriptor, false);

                bool generateAsync = ShouldBeAsync(method, ForceAsync);
                if (generateAsync)
                {
                    WriteLine();
                }
                else
                {
                    if (!Equals(method, methods.Last()))
                    {
                        WriteLine();
                    }
                }

                if (generateAsync)
                {
                    GenerateMethod(classGenerator, descriptor, true);
                    WriteLine();
                }
            }
        }

        private void GenerateMethod(ClassGenerator classGenerator, MethodDescriptor descriptor, bool forceAsync)
        {
            classGenerator.WriteMethod(
                descriptor.Method,
                forceAsync,
                g =>
                {
                    MethodInfo method = descriptor.Method;
                    ParameterInfo cancellation = descriptor.GetCancellationTokenParameter();

                    GenerateRequestCodeResult result = GenerateRequestCode(
                        descriptor,
                        descriptor.Method.GetParameters().ToDictionary(p => p, p => p.Name));

                    if (HasReturnValue(method))
                    {
                        if (IsAsync(method))
                        {
                            WriteLine(
                                "return SendAsync<{0}, {1}>({2}, {3});",
                                FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ?? method.ReturnType),
                                result.TypeName,
                                result.VariableName,
                                DeclareEndpoint(descriptor, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine(
                                "return SendAsync<{0}, {1}>({2}, {3});",
                                FormatType(method.ReturnType),
                                result.TypeName,
                                result.VariableName,
                                DeclareEndpoint(descriptor, cancellation));
                        }
                        else
                        {
                            WriteLine(
                                "return Send<{0}, {1}>({2}, {3});",
                                FormatType(method.ReturnType),
                                result.TypeName,
                                result.VariableName,
                                DeclareEndpoint(descriptor, cancellation));
                        }
                    }
                    else
                    {
                        if (IsAsync(method))
                        {
                            WriteLine("return SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine("return SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                        else
                        {
                            WriteLine("Send({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                    }
                });
        }

        protected virtual GenerateRequestCodeResult GenerateRequestCode(MethodDescriptor methodDescriptor, Dictionary<ParameterInfo, string> variables)
        {
            if (!methodDescriptor.HasParameters())
            {
                return new GenerateRequestCodeResult()
                {
                    VariableName = FormatType<Empty>() + ".Instance",
                    TypeName = FormatType<Empty>()
                };
            }

            AddUsings(methodDescriptor.Parameters.Namespace);
            WriteLine("var request = new {0}();", methodDescriptor.Parameters.Name);

            foreach (ParameterInfo info in methodDescriptor.GetParameters())
            {
                WriteLine("request.{0} = {1};", info.Name.CapitalizeFirstLetter(), variables[info]);
            }

            return new GenerateRequestCodeResult()
            {
                VariableName = "request",
                TypeName = methodDescriptor.Parameters.Name
            };
        }

        protected class GenerateRequestCodeResult
        {
            public string VariableName { get; set; }

            public string TypeName { get; set; }
        }

        private string DeclareEndpoint(MethodDescriptor descriptor, ParameterInfo cancellationTokenParameter)
        {
            WriteLine("var descriptor = {0}.{1};", ContractDescriptorProperty, descriptor.Name);
            if (cancellationTokenParameter == null)
            {
                WriteLine("var token = GetCancellationToken(descriptor);");
                WriteLine();
                return "descriptor, token";
            }

            return string.Format("descriptor, {0}", cancellationTokenParameter.Name);
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            ClassDescriptor descriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);
            string descriptorProvider = string.Format("Bolt.Client.IContractDescriptorProvider<{0}>", descriptor.Name);

            return new ClassDescriptor(
                ContractDefinition.Name + "Channel",
                ContractDefinition.Namespace,
                FormatType<Channel>(),
                ContractDefinition.Root.FullName,
                descriptorProvider);
        }
    }
}
