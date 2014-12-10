using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
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

            foreach (KeyValuePair<string, string> pair in properties)
            {
                PropertyInfo property =
                    generator.GetType()
                        .GetRuntimeProperties()
                        .FirstOrDefault(
                            p => string.Equals(p.Name, pair.Key, StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Property '{0}' not found on generator '{1}'.",
                        pair.Key,
                        generator.GetType().Name);

                    continue;
                }

                property.SetValue(generator, Convert.ChangeType(pair.Value, property.PropertyType));
            }

            return generator;
        }
    }
}