using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Bolt.Server
{
    public class QueryParameterBinder : IQueryParameterBinder
    {
        private readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> _fastSetters = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

        public virtual T BindParameters<T>(ServerActionContext context)
        {
            try
            {
                if (!context.Action.HasParameters)
                {
                    return default(T);
                }

                var query = context.Context.Request.Query;
                var parameters = Activator.CreateInstance<T>();

                var properties = PropertyHelper.GetProperties(typeof(T));
                if (properties.Length == 1)
                {
                    var target = properties[0];
                    if (!CanConvert(target.Property.PropertyType))
                    {
                        // special case, we have just single parameter and complex class, we will bind the complex class properties 
                        var innerValue = Activator.CreateInstance(target.Property.PropertyType);
                        GetSetter(target.Property)(parameters, innerValue);
                        UpdateProperties(innerValue, query, PropertyHelper.GetProperties(target.Property.PropertyType));
                        return parameters;
                    }
                }

                UpdateProperties(parameters, query, properties);
                return parameters;
            }
            catch (Exception e)
            {
                throw new DeserializeParametersException($"Failed to bind parameters for action '{context.Action}' from query '{context.Context.Request.QueryString}'.", e);
            }
        }

        private void UpdateProperties(object instance, IReadableStringCollection query, IEnumerable<PropertyHelper> properties)
        {
            foreach (var prop in properties)
            {
                // TODO: support arrays
                if (!CanConvert(prop.Property.PropertyType))
                {
                    continue;
                }

                var propValue = query[prop.Name];
                if (string.IsNullOrEmpty(propValue))
                {
                    continue;
                }

                try
                {
                    var val = ConvertSimpleType(CultureInfo.CurrentCulture, propValue, prop.Property.PropertyType);
                    GetSetter(prop.Property)(instance, val);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Failed to convert value '{propValue}' of parameter '{prop.Name}' to target type '{prop.Property.PropertyType}'.", e);
                }
            }
        }

        private bool CanConvert(Type type)
        {
            return IsSimpleType(type) || HasStringConverter(type);
        }

        private Action<object, object> GetSetter(PropertyInfo property)
        {
            return _fastSetters.GetOrAdd(property, (p) => PropertyHelper.MakeFastPropertySetter(property));
        }

        private object ConvertSimpleType(CultureInfo culture, object value, Type destinationType)
        {
            if (value == null || value.GetType().IsAssignableFrom(destinationType))
            {
                return value;
            }

            // In case of a Nullable object, we try again with its underlying type.
            destinationType = UnwrapNullableType(destinationType);

            // if this is a user-input value but the user didn't type anything, return no value
            var valueAsString = value as string;
            if (valueAsString != null && string.IsNullOrWhiteSpace(valueAsString))
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(destinationType);
            var canConvertFrom = converter.CanConvertFrom(value.GetType());
            if (!canConvertFrom)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!(canConvertFrom || converter.CanConvertTo(destinationType)))
            {
                // EnumConverter cannot convert integer, so we verify manually
                if (destinationType.GetTypeInfo().IsEnum && (value is int))
                {
                    return Enum.ToObject(destinationType, (int)value);
                }

                throw new InvalidOperationException($"No converter exists tha can convert value from source type '{value.GetType()}' to target type '{destinationType}'.");
            }

            try
            {
                return canConvertFrom
                           ? converter.ConvertFrom(null, culture, value)
                           : converter.ConvertTo(null, culture, value, destinationType);
            }
            catch (Exception ex)
            {
                if (ex is FormatException)
                {
                    throw ex;
                }
                else
                {
                    // TypeConverter throws System.Exception wrapping the FormatException,
                    // so we throw the inner exception.
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

                    // this code is never reached because the previous line is throwing;
                    throw;
                }
            }
        }

        private static Type UnwrapNullableType(Type destinationType)
        {
            return Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        }

        private static bool IsSimpleType(Type type)
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

        private static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }
    }
}