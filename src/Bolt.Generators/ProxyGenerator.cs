using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public partial class ProxyGenerator : ContractGeneratorBase
    {
        private string _contractDescriptorProperty;

        public ProxyGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ProxyGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
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
                    return $"{ContractDefinition.Name}Descriptor";
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
            AddUsings(BoltConstants.Client.Namespace, BoltConstants.Client.ChannelsNamespace, "System.Threading", "System.Reflection");

            ClassGenerator generator = CreateClassGenerator(ContractDescriptor);
            generator.Modifier = Modifier;
            generator.UserGenerator = UserGenerator;
            generator.GenerateBodyAction = g =>
                {
                    g.GenerateConstructor(g.Descriptor.FullName + " proxy", "proxy");
                    g.GenerateConstructor($"{FormatType(BoltConstants.Client.Channel)} channel", $"typeof({ContractDefinition.Root.FullName}), channel");

                    List<Type> contracts = ContractDefinition.GetEffectiveContracts().ToList();
                    foreach (Type type in contracts)
                    {
                        GenerateMethods(g, type);
                    }

                    WriteLine();
                    GenerateStaticActions(g);
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
                                "return SendAsync<{0}>({1}, {2});",
                                FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ?? method.ReturnType),
                                GetStaticActionName(method),
                                DeclareEndpoint(result.VariableName, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine(
                                "return SendAsync<{0}>({1}, {2});",
                                FormatType(method.ReturnType),
                                GetStaticActionName(method),
                                DeclareEndpoint(result.VariableName, cancellation));
                        }
                        else
                        {
                            WriteLine(
                                "return Send<{0}>({1}, {2});",
                                FormatType(method.ReturnType),
                                GetStaticActionName(method),
                                DeclareEndpoint(result.VariableName, cancellation));
                        }
                    }
                    else
                    {
                        if (IsAsync(method))
                        {
                            WriteLine("return SendAsync({0}, {1});", GetStaticActionName(method), DeclareEndpoint(result.VariableName, cancellation));
                        }
                        else if (forceAsync)
                        {
                            WriteLine("return SendAsync({0}, {1});", GetStaticActionName(method), DeclareEndpoint(result.VariableName, cancellation));
                        }
                        else
                        {
                            WriteLine("Send({0}, {1});", GetStaticActionName(method), DeclareEndpoint(result.VariableName, cancellation));
                        }
                    }
                });
        }

        protected virtual GenerateRequestCodeResult GenerateRequestCode(MethodDescriptor methodDescriptor, Dictionary<ParameterInfo, string> variables)
        {
            if (!methodDescriptor.GetParameters().Any())
            {
                return new GenerateRequestCodeResult
                {
                    VariableName = "null"
                };
            }

            WriteLine("var bolt_Params = Channel.Serializer.CreateSerializer();");

            foreach (ParameterInfo info in methodDescriptor.GetParameters())
            {
                WriteLine($"bolt_Params.WriteParameter({GetStaticActionName(methodDescriptor.Method)}, \"{info.Name}\", typeof({FormatType(info.ParameterType)}), {variables[info]});");
            }

            return new GenerateRequestCodeResult
            {
                VariableName = "bolt_Params",
            };
        }

        protected class GenerateRequestCodeResult
        {
            public string VariableName { get; set; }
        }

        private string DeclareEndpoint(string parameters, ParameterInfo cancellationTokenParameter)
        {
            if (cancellationTokenParameter == null)
            {
                return $"{parameters}, CancellationToken.None";
            }

            return $"{parameters}, {cancellationTokenParameter.Name}";
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            List<string> baseClasses = new List<string>();
            string baseClass = $"ContractProxy";
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

        private void GenerateStaticActions(ClassGenerator generator)
        {
            foreach (MethodInfo effectiveMethod in ContractDefinition.GetEffectiveMethods())
            {
                generator.WriteLine(
                    "private static readonly {0} {1} = typeof({2}).GetMethod(nameof({2}.{3}));",
                    FormatType<MethodInfo>(),
                    GetStaticActionName(effectiveMethod),
                    FormatType(effectiveMethod.DeclaringType),
                    effectiveMethod.Name);
            }
        }

        private static string GetStaticActionName(MethodInfo effectiveMethod)
        {
            return $"__{effectiveMethod.Name}Action";
        }
    }
}
