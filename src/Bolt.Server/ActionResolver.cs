using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ActionResolver : IActionResolver
    {
        public MethodInfo Resolve(Type contract, string actionName)
        {
            List<MethodInfo> candidates = contract.GetRuntimeMethods().ToList();
            return Resolve(candidates, actionName, false) ?? Resolve(candidates, actionName, true);
        }

        private MethodInfo Resolve(List<MethodInfo> candidates, string actionName, bool lowerCase)
        {
            if (lowerCase)
            {
                actionName = actionName.ToLowerInvariant();
            }
            BoltFramework.TrimAsyncPostfix(actionName, out actionName);

            return SelectByPriority(candidates.Where(m => CoerceMethodName(m.Name, lowerCase) == actionName));
        }

        private string CoerceMethodName(string name, bool lowerCase)
        {
            BoltFramework.TrimAsyncPostfix(name, out name);
            if (lowerCase)
            {
                return name.ToLowerInvariant();
            }

            return name;
        }

        private MethodInfo SelectByPriority(IEnumerable<MethodInfo> candidates)
        {
            return candidates.OrderBy(m =>
            {
                if (typeof(Task).IsAssignableFrom(m.ReturnType))
                {
                    return 0;
                }

                return 1;
            }).FirstOrDefault();
        }
    }
}