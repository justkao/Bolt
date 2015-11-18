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

        protected override ActionMetadata Create(MethodInfo method, object context)
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

            descriptor.Timeout = GetTimeout(method);
            return descriptor;
        }

        private static TimeSpan GetTimeout(MethodInfo method)
        {
            TimeSpan? timeoutValue = (from attr in
                method.GetCustomAttributes<Attribute>()
                    .Where(a => a.GetType().Name == typeof (TimeoutAttribute).Name)
                let timeoutProperty = attr.GetType().GetRuntimeProperty(nameof(TimeoutAttribute.Timeout))
                where timeoutProperty != null
                let timeout = CreateTimeout(timeoutProperty.GetValue(attr))
                where timeout != null
                select timeout).FirstOrDefault();

            if (timeoutValue != null)
            {
                return timeoutValue.Value;
            }

            return TimeSpan.Zero;
        }

        private static TimeSpan? CreateTimeout(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is TimeSpan)
            {
                return (TimeSpan)value;
            }

            if (value is int)
            {
                return TimeSpan.FromMilliseconds((int)value);
            }

            if (value is double)
            {
                return TimeSpan.FromMilliseconds((double)value);
            }

            if (value is short)
            {
                return TimeSpan.FromMilliseconds((short)value);
            }

            if (value is long)
            {
                return TimeSpan.FromMilliseconds((long)value);
            }

            return null;
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