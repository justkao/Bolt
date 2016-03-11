using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Serialization
{
    public class WriteParametersContext : SerializeContext
    {
        public WriteParametersContext(Stream stream, ActionContextBase actionContext, IList<ParameterValue> parameterValues) : base(stream, actionContext)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException(nameof(parameterValues));
            }

            ParameterValues = parameterValues;
        }

        public IList<ParameterValue> ParameterValues { get;}
    }
}