using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Generators
{
    public class ClassGenerator : GeneratorBase
    {
        public ClassGenerator(ClassDescriptor descriptor, StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            Descriptor = descriptor;
        }

        public ClassDescriptor Descriptor { get; private set; }

        public virtual void WritePublicProperty(string type, string name)
        {
            WriteLine("public virtual {0} {1} {{ get; set; }}", type, name);
        }

        public virtual void WritePublicReadonlyProperty(string type, string name)
        {
            WriteLine("public virtual {0} {1} {{ get; private set; }}", type, name);
        }

        public virtual void WriteAttribute<T>()
        {
            WriteAttribute(FormatType<T>());
        }

        public virtual void WriteAttribute(string name)
        {
            WriteLine("[{0}]", name);
        }

        public void WriteMethod(string declaration, Action<ClassGenerator> bodyGenerator, string modifier = "public virtual", string returnValue = null)
        {
            WriteLine("{0} {1}", modifier, declaration);
            using (WithBlock())
            {
                bodyGenerator(this);

                if (!string.IsNullOrEmpty(returnValue))
                {
                    WriteLine(returnValue);
                }
            }
        }

        public void WriteMethod(MethodInfo method, bool forceAsync, Action<ClassGenerator> bodyGenerator, string modifier = "public virtual", string returnValue = null)
        {
            WriteMethod(FormatMethodDeclaration(method, forceAsync), bodyGenerator, modifier, returnValue);
        }

        public virtual void GenerateClass(Action<ClassGenerator> bodyGenerator, Action<ClassGenerator> annotateClassAction = null, bool addNamespace = true)
        {
            AddUsings(Descriptor.Namespace);

            using (addNamespace ? WithNamespace(Descriptor.Namespace) : new EasyDispose(() => { }))
            {
                if (annotateClassAction != null)
                {
                    annotateClassAction(this);
                }

                string type = Descriptor.IsInterface ? "interface" : "class";

                if (Descriptor.BaseClasses.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in Descriptor.BaseClasses)
                    {
                        sb.AppendFormat("{0}, ", s);
                    }

                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                    }

                    WriteLine("public partial {0} {1} : {2}", type, Descriptor.Name, sb.ToString());
                }
                else
                {
                    WriteLine("public partial {0} {1}", type, Descriptor.Name);
                }

                using (WithBlock())
                {
                    bodyGenerator(this);
                }
            }

            WriteLine();
        }
    }
}