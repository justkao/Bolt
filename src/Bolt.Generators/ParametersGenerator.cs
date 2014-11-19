using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Bolt.Generators
{
    public class ParametersGenerator : GeneratorBase
    {
        public ParametersGenerator(StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(output, formatter, provider)
        {
        }

        public string BaseClass { get; set; }

        public virtual string GenerateInvocationCode(string instanceName, string parametersInstance, MethodInfo method)
        {
            string parametersBody = string.Empty;

            if (HasParameters(method))
            {
                StringBuilder sb = new StringBuilder();

                foreach (ParameterInfo info in method.GetParameters())
                {
                    sb.AppendFormat("{0}.{1}, ", parametersInstance, info.Name.CapitalizeFirstLetter());
                }
                sb.Remove(sb.Length - 2, 2);
                parametersBody = sb.ToString();
            }

            if (HasReturnValue(method))
            {
                if (IsAsync(method))
                {
                    WriteLine("var result = await {0}.{1}({2});", instanceName, method.Name, parametersBody);
                }
                else
                {
                    WriteLine("var result = {0}.{1}({2});", instanceName, method.Name, parametersBody);
                }

                return "result";
            }

            if (IsAsync(method))
            {
                WriteLine("await {0}.{1}({2});", instanceName, method.Name, parametersBody);
            }
            else
            {
                WriteLine("{0}.{1}({2});", instanceName, method.Name, parametersBody);
            }

            return null;
        }

        public virtual GenerateRequestCodeResult GenerateRequestCode(MethodInfo method, Dictionary<ParameterInfo, string> variables, IMetadataProvider provider)
        {
            if (!HasParameters(method))
            {
                return new GenerateRequestCodeResult()
                {
                    VariableName = FormatType<Empty>() + ".Instance",
                    TypeName = FormatType<Empty>()
                };
            }

            ParametersDescriptor descriptor = provider.GetParametersClass(method.DeclaringType, method);
            AddUsings(descriptor.Namespace);
            string typeName = descriptor.Name;

            WriteLine("var request = new {0}();", typeName);

            foreach (ParameterInfo info in method.GetParameters())
            {
                WriteLine("request.{0} = {1};", info.Name.CapitalizeFirstLetter(), variables[info]);
            }

            return new GenerateRequestCodeResult()
            {
                VariableName = "request",
                TypeName = typeName
            };
        }

        public virtual bool Generate(MethodInfo method, IMetadataProvider provider)
        {
            if (!HasParameters(method))
            {
                return false;
            }

            AddUsings(typeof(DataContractAttribute).Namespace);
            AddUsings(typeof(DataMemberAttribute).Namespace);

            WriteLine("[{0}]", FormatType<DataContractAttribute>());
            if (string.IsNullOrEmpty(BaseClass))
            {
                WriteLine("public partial class {0}", provider.GetParametersClass(method.DeclaringType, method).Name);
            }
            else
            {
                WriteLine("public partial class {0} : {1}", provider.GetParametersClass(method.DeclaringType, method).Name, BaseClass);
            }

            BeginBlock();

            int order = 1;
            foreach (ParameterInfo info in method.GetParameters())
            {
                WriteLine("[{0}(Order = {1})]", FormatType<DataMemberAttribute>(), order);
                WriteLine(FormatPublicProperty(info.ParameterType, info.Name.CapitalizeFirstLetter()));
                order++;

                if (info != method.GetParameters().Last())
                {
                    WriteLine();
                }
            }

            EndBlock();

            return true;
        }
    }
}