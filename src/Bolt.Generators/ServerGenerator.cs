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
            generator.Contract = definition;
            generator.Generate();

            return generator.Output.GetStringBuilder().ToString();
        }

        public string ServerNamespace { get; set; }

        public string ExecutorClassType { get; set; }

        public override void Generate()
        {
            AddUsings(BoltServerNamespace);
            BeginNamespace(ServerNamespace ?? Contract.Namespace);

            string name = ExecutorClassType ?? string.Format("{0}{1}", Contract.Name, ExecutorName);

            WriteLine("public partial class {0} : {1}", name, ExecutorFullName);
            BeginBlock();

            WriteLine("public override void Init()");
            BeginBlock();

            TypeDescriptor contractDescriptor = MetadataProvider.GetContractDescriptor(Contract);

            WriteLine("if (ContractDescriptor == null)");
            BeginBlock();
            WriteLine("ContractDescriptor = {0}.Default;", contractDescriptor.FullName);
            EndBlock();
            WriteLine();

            foreach (MethodInfo method in Contract.GetEffectiveMethods())
            {
                WriteLine("AddAction(ContractDescriptor.{0}, {1});", MetadataProvider.GetMethodDescriptor(Contract, method).Name, FormatMethodName(method));
            }
            WriteLine();
            WriteLine("base.Init();");
            EndBlock();

            WriteLine();
            WriteLine("public {0} ContractDescriptor {{ get; set; }}", contractDescriptor.FullName);
            WriteLine();

            ParametersGenerator generator = new ParametersGenerator(Output, Formatter, IntendProvider);
            IEnumerable<MethodInfo> methods = Contract.GetEffectiveMethods().ToList();

            foreach (MethodInfo method in methods)
            {
                WriteLine("protected virtual async {2} {0}({1} context)", FormatMethodName(method), ServerExecutionContext, FormatType<Task>());
                BeginBlock();

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

                EndBlock();

                if (method != methods.Last())
                {
                    WriteLine();
                }
            }

            EndBlock();
            EndNamespace();
            WriteLine();

            ExecutorClassType = name;
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