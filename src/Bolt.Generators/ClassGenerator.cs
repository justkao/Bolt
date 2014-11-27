using System;
using System.Collections.Generic;
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
            Modifier = "public";
        }

        public ClassDescriptor Descriptor { get; private set; }

        public string Modifier { get; set; }

        public virtual void WritePublicProperty(string type, string name)
        {
            WriteLine("public virtual {0} {1} {{ get; set; }}", type, name);
        }

        public virtual void WritePublicReadonlyProperty(string type, string name, bool isVirtual = true)
        {
            WriteLine("public {2} {0} {1} {{ get; private set; }}", type, name, isVirtual ? "virtual" : string.Empty);
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

        public virtual void GenerateClass(Action<ClassGenerator> bodyGenerator, Action<ClassGenerator> annotateClassAction = null, bool addNamespace = true, Action<ClassGenerator> introduceFields = null)
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

                    WriteLine("{3} partial {0} {1} : {2}", type, Descriptor.Name, sb.ToString(), Modifier);
                }
                else
                {
                    WriteLine("{2} partial {0} {1}", type, Descriptor.Name, Modifier);
                }

                using (WithBlock())
                {
                    if (introduceFields != null)
                    {
                        introduceFields(this);
                        WriteLine();
                    }

                    if (!Descriptor.IsInterface)
                    {
                        GenerateConstructors();
                    }

                    bodyGenerator(this);
                }
            }

            WriteLine();
        }


        public virtual void GenerateConstructor(string parameters, string baseCall = null, string modifier = "public")
        {
            if (!string.IsNullOrEmpty(baseCall))
            {
                WriteLine("{0} {1}({2}) : base({3})", modifier, Descriptor.Name, parameters, baseCall);
            }
            else
            {
                WriteLine("{0} {1}({2})", modifier, Descriptor.Name, parameters);
            }
            using (WithBlock())
            {
            }

            WriteLine();
        }

        private void GenerateConstructors()
        {
            Type baseClass =
                Descriptor.BaseClasses.EmptyIfNull()
                    .Select(n => Formatter != null ? Formatter.GetType(n) : Type.GetType(n))
                    .FirstOrDefault(t => t != null && !t.GetTypeInfo().IsInterface);

            if (baseClass == null)
            {
                return;
            }

            IEnumerable<ConstructorInfo> constructors = baseClass.GetTypeInfo().DeclaredConstructors.ToList();

            foreach (ConstructorInfo constructor in constructors)
            {
                WriteLine("{0} {1}({2}) : base({3})", constructor.IsPublic ? "public" : "protected", Descriptor.Name, FormatMethodParameters(constructor.GetParameters(), true), FormatMethodParameters(constructor.GetParameters(), false));
                using (WithBlock())
                {
                }

                WriteLine();
            }
        }
    }
}