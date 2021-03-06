﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Metadata
{
    public class ActionMetadataProvider : ValueCache<MethodInfo, ActionMetadata>, IActionMetadataProvider
    {
        private static readonly Type TaskGenericType = typeof(Task<>);

        public ActionMetadata Resolve(MethodInfo action)
        {
            return Get(action);
        }

        protected override ActionMetadata Create(MethodInfo method, object context)
        {
            ParameterInfo[] parameters = method.GetParameters();

            ActionMetadata descriptor = new ActionMetadata(method, parameters.Select(p => new ParameterMetadata(p.ParameterType, p.Name)).ToArray(), GetResultType(method));
            descriptor.Timeout = GetTimeout(method);
            return descriptor;
        }

        private static TimeSpan GetTimeout(MethodInfo method)
        {
            TimeSpan? timeoutValue = (from attr in
                method.GetCustomAttributes<Attribute>()
                    .Where(a => a.GetType().Name == typeof(TimeoutAttribute).Name)
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

        private static Type GetResultType(MethodInfo method)
        {
            if (typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo()))
            {
                return GetTaskInnerTypeOrNull(method.ReturnType) ?? typeof(void);
            }

            return method.ReturnType;
        }

        private static Type GetTaskInnerTypeOrNull(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetTypeInfo().GenericTypeArguments;
                if (genericTypeDefinition == TaskGenericType && genericArguments.Length == 1)
                {
                    // Only Return if there is a single argument.
                    return genericArguments[0];
                }
            }

            return null;
        }
    }
}