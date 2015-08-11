using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public class ProxyGenerator : ContractGeneratorBase
    {
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

        public bool ForceAsync { get; set; }

        public string Suffix { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Modifier { get; set; }

        public IEnumerable<string> BaseInterfaces { get; set; }

        public IUserCodeGenerator UserGenerator { get; set; }

        public override void Generate(object context)
        {
            AddUsings(BoltConstants.Client.Namespace, "System.Threading", "System.Reflection");

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
                    string parameters = g.FormatMethodParameters(method, false);

                    if (HasReturnValue(method))
                    {
                        if (IsAsync(method))
                        {
                            WriteLine(
                                "return this.SendAsync<{0}>({1}, {2});",
                                FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ?? method.ReturnType),
                                GetStaticActionName(method),
                                parameters);
                        }
                        else if (forceAsync)
                        {
                            WriteLine(
                                "return this.SendAsync<{0}>({1}, {2});",
                                FormatType(method.ReturnType),
                                GetStaticActionName(method),
                                parameters);
                        }
                        else
                        {
                            WriteLine(
                                "return this.Send<{0}>({1}, {2});",
                                FormatType(method.ReturnType),
                                GetStaticActionName(method),
                                parameters);
                        }
                    }
                    else
                    {
                        if (IsAsync(method))
                        {
                            WriteLine("return this.SendAsync({0}, {1});", GetStaticActionName(method), parameters);
                        }
                        else if (forceAsync)
                        {
                            WriteLine("return this.SendAsync({0}, {1});", GetStaticActionName(method), parameters);
                        }
                        else
                        {
                            WriteLine("this.Send({0}, {1});", GetStaticActionName(method), parameters);
                        }
                    }
                });
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            List<string> baseClasses = new List<string>();
            string baseClass = BoltConstants.Client.ProxyBase.FullName;
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
