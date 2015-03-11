using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bolt.Generators
{
    public class ParametersGenerator : ClassGenerator
    {
        public ParametersGenerator(MethodDescriptor methodDescriptor, StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(methodDescriptor.Parameters, output, formatter, provider)
        {
            MethodDescriptor = methodDescriptor;
            GenerateBodyAction = GenerateBody;
            AnnotateClassAction = g => g.WriteAttribute<DataContractAttribute>();
        }

        public MethodDescriptor MethodDescriptor { get; set; }

        public override void Generate(object context)
        {
            AddUsings(typeof(DataContractAttribute).Namespace);
            AddUsings(typeof(DataMemberAttribute).Namespace);

            base.Generate(context);
        }

        private void GenerateBody(ClassGenerator g)
        {
            int order = 1;
            IEnumerable<ParameterInfo> parameters = MethodDescriptor.GetParameters().ToList();

            foreach (ParameterInfo info in parameters)
            {
                WriteAttribute(string.Format("{0}(Order = {1})", FormatType<DataMemberAttribute>(), order));

                g.FormatPublicProperty(info.ParameterType, info.Name.CapitalizeFirstLetter());
                WriteLine(FormatPublicProperty(info.ParameterType, info.Name.CapitalizeFirstLetter()));
                order++;

                if (info != parameters.Last())
                {
                    WriteLine();
                }
            }
        }
    }
}