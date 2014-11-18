using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt
{
    public static class Extensions
    {
        public static string CapitalizeFirstLetter(this string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        public static bool IsAsync(this MethodInfo method)
        {
            return typeof(Task).IsAssignableFrom(method.ReturnType);
        }

        public static string GetAsyncName(this MethodInfo method)
        {
            if (method.Name.EndsWith("Async"))
            {
                return method.Name;
            }

            return method.Name + "Async";
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