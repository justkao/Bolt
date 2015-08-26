using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Metadata
{
    public class ActionMetadataProvider : ValueCache<MethodInfo, ActionMetadata>, IActionMetadataProvider
    {
        public ActionMetadata Resolve(MethodInfo action)
        {
            return Get(action);
        }

        protected override ActionMetadata Create(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            ActionMetadata descriptor = new ActionMetadata();
            descriptor.Action = method;
            descriptor.Parameters = parameters.Select(p => new ParameterMetadata(p.ParameterType, p.Name)).ToArray();
            descriptor.HasSerializableParameters = GetSerializableParameters(method).Any();
            descriptor.ResultType = GetResultType(method);
            descriptor.CancellationTokenIndex = -1;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsCancellationToken())
                {
                    descriptor.CancellationTokenIndex = i;
                }
            }

            return descriptor;
        }

        private static IEnumerable<ParameterInfo> GetSerializableParameters(MethodInfo method)
        {
            ParameterInfo[] p = method.GetParameters();
            if (p.Length == 0)
            {
                return Enumerable.Empty<ParameterInfo>();
            }

            return method.GetParameters().Where(info => !info.IsCancellationToken());
        }

        private static Type GetResultType(MethodInfo method)
        {
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                return TypeHelper.GetTaskInnerTypeOrNull(method.ReturnType) ?? typeof(void);
            }

            return method.ReturnType;
        }
    }
}