using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public class ClientGenerator : ContractGeneratorBase
    {
        private const string BoltClientNamespace = "Bolt.Client";
        private const string BoltChannelsNamespace = "Bolt.Client.Channels";
        private const string BoltChannelInterface = "IChannel";

        private string _contractDescriptorProperty;

        public ClientGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ClientGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            Suffix = "Proxy";
            Modifier = "public";
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

        public string Suffix { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Modifier { get; set; }

        public IEnumerable<string> BaseInterfaces { get; set; }

        public IUserCodeGenerator UserGenerator { get; set; }

        public override void Generate(object context)
        {
            AddUsings(BoltClientNamespace, BoltChannelsNamespace);

            ClassGenerator generator = CreateClassGenerator(ContractDescriptor);
            generator.Modifier = Modifier;
            generator.UserGenerator = UserGenerator;
            generator.GenerateBodyAction = g =>
                {
                    g.GenerateConstructor(g.Descriptor.FullName + " proxy", "proxy");
                    g.GenerateConstructor(string.Format("{0} channel", BoltChannelInterface), "channel");

                    List<Type> contracts = ContractDefinition.GetEffectiveContracts().ToList();
                    foreach (Type type in contracts)
                    {
                        GenerateMethods(g, type);
                    }
                };
            generator.Generate(context);

            WriteLine();
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
                                "return Channel.SendAsync<{0}, {1}>({2}, {3});",
                                FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ?? method.ReturnType),
                                result.TypeName,
                                result.VariableName,
                                DeclareEndpoint(descriptor, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine(
                                "return Channel.SendAsync<{0}, {1}>({2}, {3});",
                                FormatType(method.ReturnType),
                                result.TypeName,
                                result.VariableName,
                                DeclareEndpoint(descriptor, cancellation));
                        }
                        else
                        {
                            WriteLine(
                                "return Channel.Send<{0}, {1}>({2}, {3});",
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
                            WriteLine("return Channel.SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine("return Channel.SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                        else
                        {
                            WriteLine("Channel.Send({0}, {1});", result.VariableName, DeclareEndpoint(descriptor, cancellation));
                        }
                    }
                });
        }

        protected virtual GenerateRequestCodeResult GenerateRequestCode(MethodDescriptor methodDescriptor, Dictionary<ParameterInfo, string> variables)
        {
            if (!methodDescriptor.HasParameterClass())
            {
                return new GenerateRequestCodeResult()
                {
                    VariableName = FormatType<Empty>() + ".Instance",
                    TypeName = FormatType<Empty>()
                };
            }

            AddUsings(methodDescriptor.Parameters.Namespace);
            WriteLine("var bolt_Params = new {0}();", methodDescriptor.Parameters.Name);

            foreach (ParameterInfo info in methodDescriptor.GetParameters())
            {
                WriteLine("bolt_Params.{0} = {1};", info.Name.CapitalizeFirstLetter(), variables[info]);
            }

            return new GenerateRequestCodeResult()
            {
                VariableName = "bolt_Params",
                TypeName = methodDescriptor.Parameters.Name
            };
        }

        protected class GenerateRequestCodeResult
        {
            public string VariableName { get; set; }

            public string TypeName { get; set; }
        }

        private string DeclareEndpoint(MethodDescriptor action, ParameterInfo cancellationTokenParameter)
        {
            string descriptorReference = string.Format("Descriptor.{0}", action.Name);

            if (cancellationTokenParameter == null)
            {
                return string.Format("{0}, GetCancellationToken({1})", descriptorReference, descriptorReference);
            }

            return string.Format("{0}, {1}", descriptorReference, cancellationTokenParameter.Name);
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            ClassDescriptor contractDescriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);
            List<string> baseClasses = new List<string>();
            string baseClass = string.Format("ContractProxy<{0}>", contractDescriptor.FullName);
            baseClasses.Add(baseClass);
            baseClasses.Add(ContractDefinition.Root.FullName);

            if (BaseInterfaces != null)
            {
                baseClasses.AddRange(BaseInterfaces);
            }

            return new ClassDescriptor(
                Name ?? ContractDefinition.Name + Suffix,
                Namespace ?? ContractDefinition.Namespace,
                baseClasses.Distinct().ToArray());
        }
    }
}
