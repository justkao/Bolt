using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            string action;

            MethodInfo found;
            // we prefer async versions
            if (!TrimAsyncPostfix(actionName, out action))
            {
                action = actionName + Bolt.AsyncPostFix;
                // regular search
                found = candidates.FirstOrDefault(m => CoerceMethodName(m.Name, lowerCase) == action);
                if (found != null)
                {
                    return found;
                }
            }
            else
            {
                found = candidates.FirstOrDefault(m => CoerceMethodName(m.Name, lowerCase) == action);
                if (found != null)
                {
                    return found;
                }
            }

            // regular search
            return candidates.FirstOrDefault(m => CoerceMethodName(m.Name, lowerCase) == actionName);
        }

        private string CoerceMethodName(string name, bool lowerCase)
        {
            if (lowerCase)
            {
                return name.ToLowerInvariant();
            }

            return name;
        }

        private bool TrimAsyncPostfix(string actionName, out string coerced)
        {
            coerced = null;
            int index = actionName.IndexOf(Bolt.AsyncPostFix, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                return false;
            }

            coerced = actionName.Substring(0, index);
            return true;
        }
    }
}