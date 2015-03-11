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
                    return string.Format(
                        "{0}.{1}<{2}>",
                        BoltConstants.Server.Namespace,
                        FormatType(BoltConstants.Server.ContractInvoker),
                        MetadataProvider.GetContractDescriptor(ContractDefinition).FullName);
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
            AddUsings(BoltConstants.Server.Namespace);

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
            string declaration = string.Format("{2} {0}({1} context)", FormatMethodName(methodDescriptor.Method), BoltConstants.Server.ServerActionContext, FormatType<Task>());
            classGenerator.WriteMethod(declaration, g => WriteInvocationMethodBody(methodDescriptor), "protected virtual async");
        }

        private void GenerateInvocatorBody(ClassGenerator g)
        {
            g.WriteLine("protected override void InitActions()");
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
                WriteLine("var parameters = await DataHandler.ReadParametersAsync<{0}>(context);", methodDescriptor.Parameters.FullName);
            }

            string instanceType = FormatType(methodDescriptor.Method.DeclaringType);
            WriteLine("var instance = InstanceProvider.GetInstance<{0}>(context);", instanceType);

            if (HasReturnValue(methodDescriptor.Method))
            {
                WriteLine("{0} result;", FormatType(GetReturnType(methodDescriptor.Method)));
            }

            WriteLine();
            WriteLine("try");
            using (WithBlock())
            {
                GenerateInvocationCode("instance", "parameters", "result", methodDescriptor);
                WriteLine("InstanceProvider.ReleaseInstance(context, instance, null);", instanceType);
            }
            WriteLine("catch (Exception e)");
            using (WithBlock())
            {
                WriteLine("InstanceProvider.ReleaseInstance(context, instance, e);", instanceType);
                WriteLine("throw;");
            }

            WriteLine();

            WriteLine(
                HasReturnValue(methodDescriptor.Method)
                    ? "await ResponseHandler.Handle(context, result);"
                    : "await ResponseHandler.Handle(context);");
        }

        public virtual string GenerateInvocationCode(string instanceName, string parametersInstance, string resultVariable, MethodDescriptor method)
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
                        sb.Append("context.RequestAborted, ");
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
                    WriteLine("{3} = await {0}.{1}({2});", instanceName, method.Name, parametersBody, resultVariable);
                }
                else
                {
                    WriteLine("{3} = {0}.{1}({2});", instanceName, method.Name, parametersBody, resultVariable);
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