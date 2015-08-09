using System;
using System.Reflection;

namespace Bolt.Server
{
    public interface IActionResolver
    {
        MethodInfo Resolve(Type contract, string actionName);
    }
}
