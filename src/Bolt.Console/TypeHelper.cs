using System;
using System.Reflection;

namespace Bolt.Console
{
    internal class TypeHelper
    {
        public static Type GetTypeOrThrow(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }

            throw new InvalidOperationException(string.Format("Type '{0}' could not be loaded.", name));
        }
    }
}