using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Bolt.Common
{
    internal static class Conversion
    {
        public static object ConvertSimpleType( object value, Type destinationType)
        {
            return ConvertSimpleType(CultureInfo.CurrentCulture, value, destinationType);
        }

        public static object ConvertSimpleType(CultureInfo culture, object value, Type destinationType)
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
                // TypeConverter throws System.Exception wrapping the FormatException,
                // so we throw the inner exception.
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

                // this code is never reached because the previous line is throwing;
                throw;
            }
        }

        public static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        private static Type UnwrapNullableType(Type destinationType)
        {
            return Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        }
    }
}