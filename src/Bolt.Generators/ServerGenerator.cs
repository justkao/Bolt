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
        private const string InvokerName = "ContractInvoker";
        private const string ServerExecutionContext = "Bolt.Server.ServerActionContext";
        public const string BoltServerNamespace = "Bolt.Server";
        private string _baseClass;

        public ServerGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
            ContractDescriptorPropertyName = "Descriptor";
        }

        public ServerGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            ContractDescriptorPropertyName = "Descriptor";
            Suffix = "Invoker";
            Modifier = "public";
        }

        public string ContractDescriptorPropertyName { get; set; }

        public string BaseClass
        {
            get
            {
                if (_baseClass == null)
                {
                    return string.Format("{0}.{1}<{2}>", BoltServerNamespace, InvokerName, MetadataProvider.GetContractDescriptor(ContractDefinition).FullName);
                }

                return _baseClass;
            }

            set
            {
                _baseClass = value;
            }
        }

        public string Suffix { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Modifier { get; set; }

        public string StateFullInstanceProviderBase { get; set; }

        public IUserCodeGenerator InvocatorUserGenerator { get; set; }

        public IUserCodeGenerator ExtensionCodeGenerator { get; set; }

        public override void Generate(object context)
        {
            AddUsings(BoltServerNamespace);

            ClassGenerator classGenerator = CreateClassGenerator(ContractDescriptor);
            classGenerator.Modifier = Modifier;
            classGenerator.GenerateBodyAction = GenerateInvocatorBody;
            classGenerator.UserGenerator = InvocatorUserGenerator;
            classGenerator.Generate(context);

            ContractInvokerExtensionGenerator generator = CreateEx<ContractInvokerExtensionGenerator>();
            generator.UserGenerator = ExtensionCodeGenerator;
            generator.Modifier = Modifier;
            if (StateFullInstanceProviderBase != null)
            {
                generator.StateFullInstanceProviderBase = StateFullInstanceProviderBase;
            }
            generator.ContractInvoker = classGenerator.Descriptor;
            generator.Generate(context);
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            return new ClassDescriptor(Name ?? ContractDefinition.Name + Suffix, Namespace ?? ContractDefinition.Namespace, BaseClass);
        }

        private void WriteInvocationMethod(MethodDescriptor methodDescriptor, ClassGenerator classGenerator)
        {
            string declaration = string.Format("{2} {0}({1} context)", FormatMethodName(methodDescriptor.Method), ServerExecutionContext, FormatType<Task>());
            classGenerator.WriteMethod(declaration, g => WriteInvocationMethodBody(methodDescriptor), "protected virtual async");
        }

        private void GenerateInvocatorBody(ClassGenerator g)
        {
            g.WriteLine("public override void Init()");
            using (WithBlock())
            {
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

            IEnumerable<MethodInfo> methods = ContractDefinition.GetEffectiveMethods().ToList();

            foreach (MethodInfo method in methods)
            {
                WriteInvocationMethod(MetadataProvider.GetMethodDescriptor(ContractDefinition, method), g);

                if (!Equals(method, methods.Last()))
                {
                    WriteLine();
                }
            }
        }

        private void WriteInvocationMethodBody(MethodDescriptor methodDescriptor)
        {
            if (methodDescriptor.HasParameterClass())
            {
                AddUsings(methodDescriptor.Parameters.Namespace);
                WriteLine("var parameters = await DataHandler.ReadParametersAsync<{0}>(context);", methodDescriptor.Parameters.Name);
            }

            string instanceType = FormatType(methodDescriptor.Method.DeclaringType);
            WriteLine("var instance = InstanceProvider.GetInstance<{0}>(context);", instanceType);
            WriteLine("try");
            using (WithBlock())
            {
                string result = GenerateInvocationCode("instance", "parameters", methodDescriptor);
                if (!string.IsNullOrEmpty(result))
                {
                    WriteLine("await ResponseHandler.Handle(context, result);");
                }
                else
                {
                    WriteLine("await ResponseHandler.Handle(context);");
                }
                WriteLine("InstanceProvider.ReleaseInstance(context, instance, null);", instanceType);
            }
            WriteLine("catch (Exception e)");
            using (WithBlock())
            {
                WriteLine("InstanceProvider.ReleaseInstance(context, instance, e);", instanceType);
                WriteLine("throw;");
            }
        }

        public virtual string GenerateInvocationCode(string instanceName, string parametersInstance, MethodDescriptor method)
        {
            string parametersBody = string.Empty;

            if (method.GetAllParameters().Any())
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