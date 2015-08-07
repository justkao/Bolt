// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Common
{
    internal static class TypeHelper
    {
        private static readonly Type TaskGenericType = typeof(Task<>);

        public static Type GetTaskInnerTypeOrNull(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                if (genericArguments.Length == 1 && TaskGenericType == genericTypeDefinition)
                {
                    // Only Return if there is a single argument.
                    return genericArguments[0];
                }
            }

            return null;
        }

        public static bool IsSimpleType(Type type)
        {
            return type.GetTypeInfo().IsPrimitive ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(Guid)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan)) ||
                type.Equals(typeof(Uri));
        }

        public static bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
            {
                // Even though string implements IEnumerable, we don't really think of it
                // as a collection for the purposes of model binding.
                return false;
            }

            // We only need to look for IEnumerable, because IEnumerable<T> extends it.
            return typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}