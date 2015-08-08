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
            if (actionName.ToLowerInvariant() == BoltFramework.InitSessionAction.Name.ToLowerInvariant())
            {
                return BoltFramework.InitSessionAction;
            }

            if (actionName.ToLowerInvariant() == BoltFramework.DestroySessionAction.Name.ToLowerInvariant())
            {
                return BoltFramework.DestroySessionAction;
            }

            List<MethodInfo> candidates = contract.GetRuntimeMethods().ToList();
            MethodInfo found = Resolve(candidates, actionName, false) ?? Resolve(candidates, actionName, true);
            if (found != null)
            {
                return found;
            }

            foreach (Type iface in contract.GetTypeInfo().GetInterfaces())
            {
                candidates = iface.GetRuntimeMethods().ToList();
                found = Resolve(candidates, actionName, false) ?? Resolve(candidates, actionName, true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private MethodInfo Resolve(IReadOnlyCollection<MethodInfo> candidates, string actionName, bool lowerCase)
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