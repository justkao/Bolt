using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bolt.Generators
{
    public abstract class GeneratorBase
    {
        protected GeneratorBase()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        protected GeneratorBase(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
        {
            Formatter = formatter;
            IntendProvider = intendProvider;
            Output = output;
            Intend = "    ";
        }

        public StringWriter Output { get; set; }

        public string Intend { get; set; }

        public TypeFormatter Formatter { get; set; }

        public IntendProvider IntendProvider { get; set; }

        public T Create<T>() where T : GeneratorBase, new()
        {
            T res = new T();
            res.Output = Output;
            res.Formatter = Formatter;
            res.IntendProvider = IntendProvider;

            return res;
        }

        public virtual void Generate(object context)
        {
        }

        public virtual void GenerateUsings()
        {
            foreach (string ns in Formatter.GetNamespaces())
            {
                WriteLine("using {0};", ns);
            }

            WriteLine();
        }

        public virtual string FormatMethodParameters(IEnumerable<ParameterInfo> parameters, bool includeTypes)
        {
            return FormatMethodParameters(
                includeTypes,
                parameters.Select(v => new KeyValuePair<string, Type>(v.Name, v.ParameterType)).ToArray());
        }

        public virtual string FormatMethodParameters(MethodInfo method, bool includeTypes)
        {
            return FormatMethodParameters(method.GetParameters(), includeTypes);
        }

        public virtual bool HasParameters(MethodInfo method)
        {
            return method.GetParameters().Any();
        }

        public virtual bool HasReturnValue(MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                return false;
            }

            if (method.ReturnType == typeof(Task))
            {
                return false;
            }

            return true;
        }

        public virtual bool IsAsync(MethodInfo method)
        {
            return method.IsAsync();
        }

        public virtual string FormatPublicProperty(Type type, string name)
        {
            return string.Format("public {0} {1} {{ get; set; }}", FormatType(type), name);
        }

        public virtual string FormatMethodParameters(bool includeTypes, params KeyValuePair<string, Type>[] arguments)
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, Type> pair in arguments)
            {
                if (includeTypes)
                {
                    builder.AppendFormat("{0} {1}, ", Formatter.FormatType(pair.Value), pair.Key);
                }
                else
                {
                    builder.AppendFormat("{0}, ", pair.Key);
                }
            }

            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }

            return builder.ToString();
        }

        public virtual void AddUsings(params string[] namespaces)
        {
            foreach (string ns in namespaces)
            {
                Formatter.AddNamespace(ns);
            }
        }

        public virtual string FormatMethodInvocation(MethodInfo info)
        {
            if (info.ReturnType == typeof(void))
            {
                return string.Format("{0}({1})", info.Name, FormatMethodParameters(info, false));
            }

            return string.Format("{0}({1})", info.Name, FormatMethodParameters(info, false));
        }

        public virtual string FormatMethodDeclaration(MethodInfo info, bool forceAsync = false)
        {
            if (info.ReturnType == typeof(void))
            {
                if (forceAsync)
                {
                    return string.Format("{2} {0}({1})", info.GetAsyncName(), FormatMethodParameters(info, true), FormatType<Task>());
                }

                return string.Format("void {0}({1})", info.Name, FormatMethodParameters(info, true));
            }

            if (forceAsync && !info.IsAsync())
            {
                return string.Format("{3}<{0}> {1}({2})", FormatType(info.ReturnType), info.GetAsyncName(), FormatMethodParameters(info, true), FormatType<Task>());
            }

            return string.Format("{0} {1}({2})", FormatType(info.ReturnType), info.Name, FormatMethodParameters(info, true));
        }

        public virtual string FormatType<T>()
        {
            return Formatter.FormatType(typeof(T));
        }

        public virtual string FormatType(Type type)
        {
            return Formatter.FormatType(type);
        }

        public virtual void WriteLine(string format, params object[] parameters)
        {
            Write(format, parameters);
            Output.WriteLine();
        }

        public virtual void Write(string format, params object[] parameters)
        {
            Output.Write(GetIntend());

            if (parameters.Any())
            {
                Output.Write(format, parameters);
            }
            else
            {
                Output.Write(format);
            }
        }

        public virtual void WriteLine()
        {
            Output.WriteLine();
        }


        public virtual void BeginNamespace(string name)
        {
            WriteLine("namespace {0}", name);
            BeginBlock();
        }

        public virtual void EndNamespace()
        {
            EndBlock();
        }

        public virtual IDisposable WithNamespace(string name)
        {
            BeginNamespace(name);

            return new EasyDispose(EndNamespace);
        }

        public virtual IDisposable WithBlock()
        {
            BeginBlock();

            return new EasyDispose(EndBlock);
        }

        public virtual void BeginBlock()
        {
            WriteLine("{");
            IntendProvider.Intend++;
        }

        public virtual void EndBlock()
        {
            IntendProvider.Intend--;
            WriteLine("}");
        }

        protected virtual string GetIntend()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < IntendProvider.Intend; i++)
            {
                sb.Append(Intend);
            }

            return sb.ToString();
        }
    }
}