using System;
using System.Reflection;

namespace Bolt
{
    public struct ParameterValue
    {
        private object _value;

        public ParameterValue(ParameterMetadata parameter, object value)
        {
            Parameter = parameter;
            _value = value;
        }

        public ParameterMetadata Parameter { get; }

        public object Value
        {
            get
            {
                if (_value != null)
                {
                    return _value;
                }

                if (Parameter.Type.GetTypeInfo().IsValueType)
                {
                    return Activator.CreateInstance(Parameter.Type);
                }

                return null;
            }
        }

    }
}