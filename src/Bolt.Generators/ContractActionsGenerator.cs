using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Generators
{
    public class ContractActionsGenerator : ContractGeneratorBase
    {
        private string _baseClass;

        public ContractActionsGenerator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
            ContractDescriptorPropertyName = "Descriptor";
        }

        public ContractActionsGenerator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            ContractDescriptorPropertyName = "Descriptor";
            Suffix = "Actions";
            Modifier = "public";
            GenerateExtensions = true;
        }

        public string ContractDescriptorPropertyName { get; set; }

        public string BaseClass
        {
            get
            {
                if (_baseClass == null)
                {
                    return $"{BoltConstants.Server.Namespace}.{FormatType(BoltConstants.Server.ContractActions)}<{MetadataProvider.GetContractDescriptor(ContractDefinition).FullName}>";
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

        public bool GenerateExtensions { get; set; }

        public override void Generate(object context)
        {
            AddUsings(BoltConstants.Server.Namespace);

            ClassGenerator classGenerator = CreateClassGenerator(ContractDescriptor);
            classGenerator.Modifier = Modifier;
            classGenerator.GenerateBodyAction = GenerateInvocatorBody;
            classGenerator.UserGenerator = InvocatorUserGenerator;
            classGenerator.Generate(context);

            if (GenerateExtensions)
            {
                BoltRouteHandlerExtensionsGenerator generator = CreateEx<BoltRouteHandlerExtensionsGenerator>();
                generator.UserGenerator = ExtensionCodeGenerator;
                generator.Modifier = Modifier;
                if (StateFullInstanceProviderBase != null)
                {
                    generator.StateFullInstanceProviderBase = StateFullInstanceProviderBase;
                }
                generator.ContractActions = classGenerator.Descriptor;
                generator.Generate(context);
            }
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            return new ClassDescriptor(Name ?? ContractDefinition.Name + Suffix, Namespace ?? ContractDefinition.Namespace, BaseClass);
        }

        private void WriteInvocationMethod(MethodDescriptor methodDescriptor, ClassGenerator classGenerator)
        {
            string declaration = string.Format("{2} {0}({1} context)", FormatMethodName(methodDescriptor.Method), FormatType(BoltConstants.Server.ServerActionContext), FormatType<Task>());

            if (IsAsync(methodDescriptor.Method))
            {
                classGenerator.WriteMethod(declaration, g => WriteInvocationMethodBody(methodDescriptor), "protected virtual async");
            }
            else
            {
                classGenerator.WriteMethod(declaration, g => WriteInvocationMethodBody(methodDescriptor), "protected virtual");
            }
        }

        private void GenerateInvocatorBody(ClassGenerator g)
        {
            g.WriteLine("public {0}()", g.Descriptor.Name);
            using (WithBlock())
            {
                foreach (MethodInfo method in ContractDefinition.GetEffectiveMethods())
                {
                    WriteLine(
                        "Add({0}.{1}, {2});",
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
                WriteLine("var parameters = context.GetRequiredParameters<{0}>();", methodDescriptor.Parameters.FullName);
            }

            string instanceType = FormatType(methodDescriptor.Method.DeclaringType);
            WriteLine("var instance = context.GetRequiredInstance<{0}>();", instanceType);
            GenerateInvocationCode("instance", "parameters", "result", methodDescriptor);
        }

        public virtual void GenerateInvocationCode(string instanceName, string parametersInstance, string resultVariable, MethodDescriptor method)
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
                    WriteLine("context.Result = await {1}.{2}({3});", resultVariable, instanceName, method.Name, parametersBody);
                }
                else
                {
                    WriteLine("context.Result = {1}.{2}({3});", resultVariable, instanceName, method.Name, parametersBody);
                    WriteLine("return Task.FromResult(true);");
                }

                return;
            }

            if (IsAsync(method.Method))
            {
                WriteLine("await {0}.{1}({2});", instanceName, method.Name, parametersBody);
            }
            else
            {
                WriteLine("{0}.{1}({2});", instanceName, method.Name, parametersBody);
                WriteLine("return Task.FromResult(true);");
            }
        }

        protected virtual string FormatMethodName(MethodInfo info)
        {
            if (HasParameters(info))
            {
                return $"{info.DeclaringType.StripInterfaceName()}_{info.Name}";
            }

            return $"{info.DeclaringType.StripInterfaceName()}_{info.Name}";
        }
    }
}