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
        public const string AsyncSuffix = "Async";

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
            return new T
            {
                Output = Output,
                Formatter = Formatter,
                IntendProvider = IntendProvider
            };
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

        public virtual Type GetReturnType(MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                return null;
            }

            if (method.ReturnType == typeof(Task))
            {
                return null;
            }

            if (typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo()))
            {
                return method.ReturnType.GetTypeInfo().GenericTypeArguments.First();
            }

            return method.ReturnType;
        }

        public virtual bool IsAsync(MethodInfo method)
        {
            return method.IsAsync();
        }

        public virtual string FormatPublicProperty(Type type, string name)
        {
            return $"public {FormatType(type)} {name} {{ get; set; }}";
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
                return $"{info.Name}({FormatMethodParameters(info, false)})";
            }

            return $"{info.Name}({FormatMethodParameters(info, false)})";
        }

        public virtual string FormatToSyncMethodDeclaration(MethodInfo info)
        {
            if (info.ReturnType.GetTypeInfo().IsGenericType)
            {
                return $"{FormatType(info.ReturnType.GetTypeInfo().GenericTypeArguments[0])} {info.GetSyncName()}({FormatMethodParameters(info, true)})";
            }

            return $"void {info.GetSyncName()}({FormatMethodParameters(info, true)})";
        }

        public virtual string FormatToAsyncMethodDeclaration(MethodInfo info)
        {
            if (info.ReturnType == typeof(void))
            {
                return
                    $"Task {info.GetAsyncName()}({FormatMethodParameters(info, true)})";
            }



            return
                $"Task<{FormatType(info.ReturnType)}> {info.GetAsyncName()}({FormatMethodParameters(info, true)})";
        }

        public virtual string FormatMethodDeclaration(MethodInfo info)
        {
            if (info.ReturnType == typeof(void))
            {
                return $"void {info.Name}({FormatMethodParameters(info, true)})";
            }
            
            return $"{FormatType(info.ReturnType)} {info.Name}({FormatMethodParameters(info, true)})";
        }

        public virtual string FormatMethodDeclaration(MethodInfo info, MethodDeclarationFormatting formatting)
        {
            if (formatting == MethodDeclarationFormatting.Unchanged)
            {
                return FormatMethodDeclaration(info);
            }

            if (info.IsAsync())
            {
                if (formatting == MethodDeclarationFormatting.ChangeToSync)
                {
                    return FormatToSyncMethodDeclaration(info);
                }

                return FormatMethodDeclaration(info);
            }

            if (formatting == MethodDeclarationFormatting.ChangeToAsync)
            {
                return FormatToAsyncMethodDeclaration(info);
            }

            return FormatMethodDeclaration(info);
        }

        public virtual string FormatType<T>()
        {
            return Formatter.FormatType(typeof(T));
        }

        public virtual string FormatType(ClassDescriptor descriptor)
        {
            return Formatter.FormatType(descriptor);
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