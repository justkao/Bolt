// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Common
{
    internal static class TypeExtensions
    {
        // NOTE: Do not make #105 worse! Do not add new extension methods that conflict w/ .NET 4.5 methods. The
        // exising NETFX_CORE || DNXCORE50 methods should go away (soon).
#if DNX451 || DNXCORE50 || DOTNET
        private static bool EqualTo(this Type[] t1, Type[] t2)
        {
            if (t1.Length != t2.Length)
            {
                return false;
            }

            for (var idx = 0; idx < t1.Length; ++idx)
            {
                if (t1[idx] != t2[idx])
                {
                    return false;
                }
            }

            return true;
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            return type.GetTypeInfo().DeclaredConstructors
                                     .Where(c => c.IsPublic)
                                     .SingleOrDefault(c => c.GetParameters()
                                                            .Select(p => p.ParameterType).ToArray().EqualTo(types));
        }
#endif

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface =
                t => t.IsGenericType() && t.GetGenericTypeDefinition() == interfaceType;
            return (matchesInterface(queryType)) ?
                queryType :
                queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

#if NETFX_CORE || DNXCORE50 || DOTNET
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }
#endif

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsCompatibleWith(this Type type, object value)
        {
            return (value == null && AllowsNullValue(type)) ||
                (value != null && type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()));
        }

        public static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool AllowsNullValue(this Type type)
        {
            return (!type.GetTypeInfo().IsValueType || IsNullableValueType(type));
        }

        public static Type[] GetTypeArgumentsIfMatch(Type closedType, Type matchingOpenType)
        {
            var closedTypeInfo = closedType.GetTypeInfo();
            if (!closedTypeInfo.IsGenericType)
            {
                return null;
            }

            var openType = closedType.GetGenericTypeDefinition();
            return (matchingOpenType == openType) ? closedTypeInfo.GenericTypeArguments : null;
        }
    }
}