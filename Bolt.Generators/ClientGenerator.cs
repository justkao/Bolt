using Bolt.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public class ClientGenerator : ContractGeneratorBase
    {
        public ClientGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ClientGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public static string GenerateStatefull(ContractDefinition definition, string clientNamespace)
        {
            return Generate(definition, clientNamespace, typeof(StatefullChannel).FullName);
        }

        public static string GenerateStateless(ContractDefinition definition, string clientNamespace)
        {
            return Generate(definition, clientNamespace, typeof(Channel).FullName);
        }

        public static string Generate(ContractDefinition definition, string clientNamespace, string baseClass = null)
        {
            ClientGenerator generator = new ClientGenerator();
            generator.ClientNamespace = clientNamespace;
            generator.Contract = definition;
            generator.BaseClass = baseClass;
            generator.Generate();

            return generator.Output.GetStringBuilder().ToString();
        }

        public string ClientNamespace { get; set; }

        public string BaseClass { get; set; }

        public string ClassName { get; set; }

        public bool ForceAsync { get; set; }

        public override void Generate()
        {
            BeginNamespace(ClientNamespace ?? Contract.RootContract.Namespace);

            List<Type> contracts = Contract.GetEffectiveContracts().ToList();

            WriteLine("public partial class {0} : {1}{2}, {3}", ClassName ?? Contract.RootContract.StripInterfaceName(), BaseClass != null ? BaseClass + ", " : null, typeof(IChannel).FullName, Contract.RootContract.FullName);
            BeginBlock();

            foreach (Type type in contracts)
            {
                Generate(type, MetadataProvider);
            }

            EndBlock();
            EndNamespace();
            WriteLine();
        }

        private void Generate(Type contract, IMetadataProvider provider)
        {
            ParametersGenerator generator = new ParametersGenerator(Output, Formatter, IntendProvider);
            MethodInfo[] methods = Contract.GetEffectiveMethods(contract).ToArray();

            foreach (MethodInfo method in methods)
            {
                GenerateMethod(method, generator, provider.GetRemoteUrl(Contract, method), provider, false);

                bool generateAsync = ShouldBeAsync(method, ForceAsync);

                if (generateAsync)
                {
                    WriteLine();
                }
                else
                {
                    if (method != methods.Last())
                    {
                        WriteLine();
                    }
                }

                if (generateAsync)
                {
                    GenerateMethod(method, generator, provider.GetRemoteUrl(Contract, method), provider, true);
                    WriteLine();
                }
            }
        }

        private void GenerateMethod(MethodInfo method, ParametersGenerator generator, MethodDescriptor descriptor, IMetadataProvider provider, bool forceAsync)
        {
            WriteLine("public " + FormatMethodDeclaration(method, forceAsync));
            BeginBlock();

            GenerateRequestCodeResult result = generator.GenerateRequestCode(method, method.GetParameters().ToDictionary(p => p, p => p.Name), provider);

            if (HasReturnValue(method))
            {
                if (IsAsync(method))
                {
                    WriteLine("return SendAsync<{0}, {1}>({2}, {3});", FormatType(method.ReturnType.GenericTypeArguments.FirstOrDefault() ?? method.ReturnType), result.TypeName, result.VariableName, DeclareEndpoint(descriptor));
                }
                else if (forceAsync)
                {
                    WriteLine("return SendAsync<{0}, {1}>({2}, {3});", FormatType(method.ReturnType), result.TypeName, result.VariableName, DeclareEndpoint(descriptor));
                }
                else
                {
                    WriteLine("return Send<{0}, {1}>({2}, {3});", FormatType(method.ReturnType), result.TypeName, result.VariableName, DeclareEndpoint(descriptor));
                }
            }
            else
            {
                if (IsAsync(method))
                {
                    WriteLine("return SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor));
                }
                else if (forceAsync)
                {
                    WriteLine("return SendAsync({0}, {1});", result.VariableName, DeclareEndpoint(descriptor));
                }
                else
                {
                    WriteLine("Send({0}, {1});", result.VariableName, DeclareEndpoint(descriptor));
                }
            }
            EndBlock();
        }

        private string DeclareEndpoint(MethodDescriptor descriptor)
        {
            WriteLine("var descriptor = GetEndpoint(new {3}(\"{0}\",\"{1}\",\"{2}\"));", descriptor.Contract, descriptor.Method, descriptor.Url, FormatType<MethodDescriptor>());
            WriteLine("var token = GetCancellationToken(descriptor);");
            WriteLine();
            return "descriptor, token";
        }
    }
}
