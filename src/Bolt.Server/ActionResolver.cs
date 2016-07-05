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
            var initSession = BoltFramework.SessionMetadata.Resolve(contract).InitSession;
            if (actionName.ToLowerInvariant() == initSession.Name.ToLowerInvariant())
            {
                return initSession.Action;
            }

            var destroySession = BoltFramework.SessionMetadata.Resolve(contract).DestroySession;
            if (actionName.ToLowerInvariant() == destroySession.Name.ToLowerInvariant())
            {
                return destroySession.Action;
            }

            List<MethodInfo> candidates = contract.GetRuntimeMethods().ToList();
            MethodInfo found = Resolve(candidates, actionName, false) ?? Resolve(candidates, actionName, true);
            if (found != null)
            {
                return found;
            }

            foreach (Type iface in contract.GetTypeInfo().ImplementedInterfaces)
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
                if (typeof(Task).GetTypeInfo().IsAssignableFrom(m.ReturnType.GetTypeInfo()))
                {
                    return 0;
                }

                return 1;
            }).FirstOrDefault();
        }
    }
}