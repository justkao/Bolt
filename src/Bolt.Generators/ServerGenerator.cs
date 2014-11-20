using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Bolt.Generators
{
    public class ServerGenerator : ContractGeneratorBase
    {
        private const string ExecutorName = "Executor";
        private const string ServerExecutionContext = "Bolt.Server.ServerExecutionContext";
        private const string BoltServerNamespace = "Bolt.Server";

        public ServerGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
            ContractDescriptorPropertyName = "ContractDescriptor";
        }

        public ServerGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            ContractDescriptorPropertyName = "ContractDescriptor";
        }

        public string ContractDescriptorPropertyName { get; set; }

        public override void Generate()
        {
            AddUsings(BoltServerNamespace);

            ClassDescriptor contractDescriptor = MetadataProvider.GetContractDescriptor(ContractDefinition);
            ClassGenerator classGenerator = CreateClassGenerator(ContractDescriptor);

            classGenerator.GenerateClass(
                g =>
                {
                    g.WriteLine("public override void Init()");
                    using (WithBlock())
                    {
                        WriteLine("if ({0} == null)", ContractDescriptorPropertyName);
                        using (WithBlock())
                        {
                            WriteLine("{0} = {1}.Default;", ContractDescriptorPropertyName, contractDescriptor.FullName);
                        }
                        WriteLine();

                        foreach (MethodInfo method in ContractDefinition.GetEffectiveMethods())
                        {
                            WriteLine(
                                "AddAction({0}.{1}, {2});",
                                ContractDescriptorPropertyName,
                                MetadataProvider.GetMethodDescriptor(ContractDefinition, method).Name,
                                FormatMethodName(method));
                        }

                        WriteLine();
                        WriteLine("base.Init();");
                    }


                    WriteLine();
                    g.WritePublicProperty(contractDescriptor.FullName, "ContractDescriptor");
                    WriteLine();

                    IEnumerable<MethodInfo> methods = ContractDefinition.GetEffectiveMethods().ToList();

                    foreach (MethodInfo method in methods)
                    {
                        WriteInvocationMethod(MetadataProvider.GetMethodDescriptor(ContractDefinition, method), g);

                        if (!Equals(method, methods.Last()))
                        {
                            WriteLine();
                        }
                    }
                });
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            return new ClassDescriptor(ContractDefinition.Name + ExecutorName, BoltServerNamespace, BoltServerNamespace + "." + ExecutorName);
        }

        private void WriteInvocationMethod(MethodDescriptor methodDescriptor, ClassGenerator classGenerator)
        {
            string declaration = string.Format("{2} {0}({1} context)", FormatMethodName(methodDescriptor.Method), ServerExecutionContext, FormatType<Task>());
            classGenerator.WriteMethod(declaration, g => WriteInvocationMethodBody(methodDescriptor), "protected virtual async");
        }

        private void WriteInvocationMethodBody(MethodDescriptor methodDescriptor)
        {
            if (methodDescriptor.HasParameters())
            {
                AddUsings(methodDescriptor.Parameters.Namespace);
                WriteLine("var parameters = await DataHandler.ReadParametersAsync<{0}>(context);", methodDescriptor.Parameters.Name);
            }

            string instanceType = FormatType(methodDescriptor.Method.DeclaringType);
            WriteLine("var instance = await InstanceProvider.GetInstanceAsync<{0}>(context);", instanceType);

            string result = GenerateInvocationCode("instance", "parameters", methodDescriptor);
            if (!string.IsNullOrEmpty(result))
            {
                WriteLine("await ResponseHandler.Handle(context, result);");
            }
            else
            {
                WriteLine("await ResponseHandler.Handle(context);");
            }
        }

        public virtual string GenerateInvocationCode(string instanceName, string parametersInstance, MethodDescriptor method)
        {
            string parametersBody = string.Empty;

            if (method.HasParameters())
            {
                StringBuilder sb = new StringBuilder();

                ParameterInfo cancellation = method.GetCancellationTokenParameter();
                foreach (ParameterInfo info in method.GetAllParameters())
                {
                    if (info == cancellation)
                    {
                        sb.Append("context.CallCancelled, ");
                    }
                    else
                    {
                        sb.AppendFormat("{0}.{1}, ", parametersInstance, info.Name.CapitalizeFirstLetter());
                    }
                }
                sb.Remove(sb.Length - 2, 2);
                parametersBody = sb.ToString();
            }

            if (HasReturnValue(method.Method))
            {
                if (IsAsync(method.Method))
                {
                    WriteLine("var result = await {0}.{1}({2});", instanceName, method.Name, parametersBody);
                }
                else
                {
                    WriteLine("var result = {0}.{1}({2});", instanceName, method.Name, parametersBody);
                }

                return "result";
            }

            if (IsAsync(method.Method))
            {
                WriteLine("await {0}.{1}({2});", instanceName, method.Name, parametersBody);
            }
            else
            {
                WriteLine("{0}.{1}({2});", instanceName, method.Name, parametersBody);
            }

            return null;
        }

        protected virtual string FormatMethodName(MethodInfo info)
        {
            if (HasParameters(info))
            {
                return string.Format("{0}_{1}", info.DeclaringType.StripInterfaceName(), info.Name);
            }

            return string.Format("{0}_{1}", info.DeclaringType.StripInterfaceName(), info.Name);
        }
    }
}