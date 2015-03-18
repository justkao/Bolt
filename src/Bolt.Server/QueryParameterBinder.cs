using Bolt.Common;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bolt.Server
{
    public class QueryParameterBinder : IQueryParameterBinder
    {
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
                        target.SetValue(parameters, innerValue);
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
                    var val = Conversion.ConvertSimpleType(CultureInfo.CurrentCulture, propValue, prop.Property.PropertyType);
                    prop.SetValue(instance, val);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Failed to convert value '{propValue}' of parameter '{prop.Name}' to target type '{prop.Property.PropertyType}'.", e);
                }
            }
        }

        private bool CanConvert(Type type)
        {
            return TypeHelper.IsSimpleType(type) || Conversion.HasStringConverter(type);
        }
    }
}