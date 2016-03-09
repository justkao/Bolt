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

        public bool ForceSync { get; set; }

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
                    g.GenerateConstructor($"{FormatType(BoltConstants.Client.Pipeline)} channel", $"typeof({ContractDefinition.Root.FullName}), channel");

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

                GenerateMethod(classGenerator, descriptor, MethodDeclarationFormatting.Unchanged);

                MethodDeclarationFormatting decl = MethodDeclarationFormatting.Unchanged;
                if (ShouldBeSync(method, ForceSync))
                {
                    decl = MethodDeclarationFormatting.ChangeToSync;
                }
                else if (ShouldBeAsync(method, ForceAsync))
                {
                    decl = MethodDeclarationFormatting.ChangeToAsync;
                }

                if (decl != MethodDeclarationFormatting.Unchanged)
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

                if (decl != MethodDeclarationFormatting.Unchanged)
                {
                    GenerateMethod(classGenerator, descriptor, decl);
                    WriteLine();
                }
            }
        }

        private void GenerateMethod(ClassGenerator classGenerator, MethodDescriptor descriptor,
            MethodDeclarationFormatting declaration)
        {
            classGenerator.WriteMethod(
                descriptor.Method,
                declaration,
                g =>
                {
                    MethodInfo method = descriptor.Method;
                    string parameters = g.FormatMethodParameters(method, false);
                    if (!string.IsNullOrEmpty(parameters))
                    {
                        parameters = ", " + parameters;
                    }

                    if (HasReturnValue(method))
                    {
                        MethodDeclarationFormatting targetDeclaration = method.IsAsync()
                            ? MethodDeclarationFormatting.ChangeToAsync
                            : MethodDeclarationFormatting.ChangeToSync;

                        if (declaration == MethodDeclarationFormatting.ChangeToSync)
                        {
                            targetDeclaration = MethodDeclarationFormatting.ChangeToSync;
                        }

                        if (declaration == MethodDeclarationFormatting.ChangeToAsync)
                        {
                            targetDeclaration = MethodDeclarationFormatting.ChangeToAsync;
                        }

                        switch (targetDeclaration)
                        {
                            case MethodDeclarationFormatting.ChangeToSync:
                                WriteLine(
                                    "return this.Send<{0}>({1}{2});",
                                    FormatType(method.IsAsync()
                                        ? method.ReturnType.GetTypeInfo().GenericTypeArguments[0]
                                        : method.ReturnType),
                                    GetStaticActionName(method),
                                    parameters);
                                break;
                            case MethodDeclarationFormatting.ChangeToAsync:
                                WriteLine(
                                    "return this.SendAsync<{0}>({1}{2});",
                                    FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ??
                                               method.ReturnType),
                                    GetStaticActionName(method),
                                    parameters);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        MethodDeclarationFormatting targetDeclaration = method.IsAsync()
                            ? MethodDeclarationFormatting.ChangeToAsync
                            : MethodDeclarationFormatting.ChangeToSync;

                        if (declaration == MethodDeclarationFormatting.ChangeToSync)
                        {
                            targetDeclaration = MethodDeclarationFormatting.ChangeToSync;
                        }

                        if (declaration == MethodDeclarationFormatting.ChangeToAsync)
                        {
                            targetDeclaration = MethodDeclarationFormatting.ChangeToAsync;
                        }

                        switch (targetDeclaration)
                        {
                            case MethodDeclarationFormatting.ChangeToSync:
                                WriteLine("this.Send({0}{1});", GetStaticActionName(method), parameters);
                                break;
                            case MethodDeclarationFormatting.ChangeToAsync:
                                WriteLine("return this.SendAsync({0}{1});", GetStaticActionName(method), parameters);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
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

            return new ClassDescriptor(Name ?? ContractDefinition.Name + Suffix, Namespace ?? ContractDefinition.Namespace, baseClasses.Distinct().ToArray());
        }

        private void GenerateStaticActions(ClassGenerator generator)
        {
            foreach (MethodInfo effectiveMethod in ContractDefinition.GetEffectiveMethods())
            {
                generator.WriteLine("private static readonly {0} {1} = typeof({2}).GetMethod(nameof({2}.{3}));", FormatType<MethodInfo>(), GetStaticActionName(effectiveMethod), FormatType(effectiveMethod.DeclaringType), effectiveMethod.Name);
            }
        }

        private static string GetStaticActionName(MethodInfo effectiveMethod)
        {
            return $"__{effectiveMethod.Name}Action";
        }
    }
}
