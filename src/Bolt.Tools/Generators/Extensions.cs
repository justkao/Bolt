using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Tools.Generators
{
    public static class Extensions
    {
        public static string TrimEnd(this string text, string suffix)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(suffix))
            {
                return text;
            }

            if (!text.EndsWith(suffix))
            {
                return text;
            }

            int index = text.LastIndexOf(suffix, StringComparison.Ordinal);
            if (index < 0)
            {
                return text;
            }

            return text.Substring(0, index);
        }

        public static string LowerCaseFirstLetter(this string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

        public static string CapitalizeFirstLetter(this string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        public static bool IsAsync(this MethodInfo method)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo());
        }

        public static Type GetTargetRawType(this MethodInfo method)
        {
            if (method.IsAsync())
            {
                if (typeof(Task) == method.ReturnType)
                {
                    return typeof(void);
                }

                return method.ReturnType.GetTypeInfo().GenericTypeArguments[0];
            }

            return method.ReturnType;
        }

        public static string GetSyncName(this MethodInfo method)
        {
            return method.Name.TrimEnd(GeneratorBase.AsyncSuffix);
        }

        public static string GetAsyncName(this MethodInfo method)
        {
            if (method.Name.EndsWith(GeneratorBase.AsyncSuffix))
            {
                return method.Name;
            }

            return method.Name + GeneratorBase.AsyncSuffix;
        }

        public static string StripInterfaceName(this Type type)
        {
            if (type.Name[0] == 'I')
            {
                return type.Name.Substring(1);
            }

            return type.Name;
        }
    }
}