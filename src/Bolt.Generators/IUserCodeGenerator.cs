using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Generators
{
    using Microsoft.Framework.Internal;

    public interface IUserCodeGenerator
    {
        void Generate(ClassGenerator generator, object context);
    }

    public static class UserCodeGeneratorExtensions
    {
        public static IUserCodeGenerator Activate(Type type, IDictionary<string, string> properties = null)
        {
            IUserCodeGenerator generator = (IUserCodeGenerator)Activator.CreateInstance(type);
            if (properties == null || !properties.Any())
            {
                return generator;
            }

            foreach (var prop in PropertyHelper.GetProperties(generator))
            {
                if (!properties.ContainsKey(prop.Name))
                {
                    continue;
                }

                prop.SetValue(generator, Convert.ChangeType(properties[prop.Name], prop.Property.PropertyType));
            }

            return generator;
        }
    }
}