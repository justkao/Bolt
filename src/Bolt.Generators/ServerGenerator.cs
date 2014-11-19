using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Generators
{
    public class ServerGenerator : ContractGeneratorBase
    {
        private const string ExecutorName = "Executor";
        private const string ExecutorFullName = BoltServerNamespace + "." + ExecutorName;
        private const string ServerExecutionContext = "Bolt.Server.ServerExecutionContext";
        private const string BoltServerNamespace = "Bolt.Server";

        public ServerGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public ServerGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public static string Generate(ContractDefinition definition, string serverNamespace)
        {
            ServerGenerator generator = new ServerGenerator();
            generator.ServerNamespace = serverNamespace;
            generator.ContractDefinition = definition;
            generator.Generate();

            return generator.Output.GetStringBuilder().ToString();
        }

        public string ServerNamespace { get; set; }

        public string ExecutorClassType { get; set; }

        public override void Generate()
        {
            AddUsings(BoltServerNamespace);
            string name = ExecutorClassType ?? string.Format("{0}{1}", ContractDefinition.Name, ExecutorName);

            using (WithNamespace(ServerNamespace ?? ContractDefinition.Namespace))
            {
                WriteLine("public partial class {0} : {1}", name, ExecutorFullName);
                using (WithBlock())
                {
                    TypeDescriptor contractDescriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);

                    WriteLine("public override void Init()");
                    using (WithBlock())
                    {
                        WriteLine("if (ContractDescriptor == null)");
                        using (WithBlock())
                        {
                            WriteLine("ContractDescriptor = {0}.Default;", contractDescriptor.FullName);
                        }
                        WriteLine();

                        foreach (MethodInfo method in ContractDefinition.GetEffectiveMethods())
                        {
                            WriteLine("AddAction(ContractDescriptor.{0}, {1});", MetadataProvider.GetMethodDescriptor(ContractDefinition, method).Name, FormatMethodName(method));
                        }

                        WriteLine();
                        WriteLine("base.Init();");
                    }


                    WriteLine();
                    WriteLine("public {0} ContractDescriptor {{ get; set; }}", contractDescriptor.FullName);
                    WriteLine();

                    ParametersGenerator generator = new ParametersGenerator(Output, Formatter, IntendProvider);
                    IEnumerable<MethodInfo> methods = ContractDefinition.GetEffectiveMethods().ToList();

                    foreach (MethodInfo method in methods)
                    {
                        WriteInvocationMethod(method, generator);

                        if (!Equals(method, methods.Last()))
                        {
                            WriteLine();
                        }
                    }
                }
            }

            WriteLine();
            ExecutorClassType = name;
        }

        private void WriteInvocationMethod(MethodInfo method, ParametersGenerator generator)
        {
            WriteLine("protected virtual async {2} {0}({1} context)", FormatMethodName(method), ServerExecutionContext, FormatType<Task>());

            using (WithBlock())
            {
                if (HasParameters(method))
                {
                    ParametersDescriptor descriptor = MetadataProvider.GetParameterDescriptor(method.DeclaringType, method);
                    AddUsings(descriptor.Namespace);
                    WriteLine("var parameters = await DataHandler.ReadParametersAsync<{0}>(context);", descriptor.Name);
                }

                WriteLine("var instance = await InstanceProvider.GetInstanceAsync<{0}>(context);", FormatType(method.DeclaringType));

                string result = generator.GenerateInvocationCode("instance", "parameters", method);
                if (!string.IsNullOrEmpty(result))
                {
                    WriteLine("await ResponseHandler.Handle(context, result);");
                }
                else
                {
                    WriteLine("await ResponseHandler.Handle(context);");
                }
            }
        }

        private string FormatMethodName(MethodInfo info)
        {
            if (HasParameters(info))
            {
                return string.Format("{0}_{1}", info.DeclaringType.StripInterfaceName(), info.Name);
            }

            return string.Format("{0}_{1}", info.DeclaringType.StripInterfaceName(), info.Name);
        }
    }
}