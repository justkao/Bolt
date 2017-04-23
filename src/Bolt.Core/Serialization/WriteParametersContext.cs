using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Serialization
{
    public class WriteParametersContext : SerializeContext
    {
        public WriteParametersContext(Stream stream, ActionContextBase actionContext, IList<ParameterValue> parameterValues) : base(stream, actionContext)
        {
            ParameterValues = parameterValues ?? throw new ArgumentNullException(nameof(parameterValues));
        }

        public IList<ParameterValue> ParameterValues { get;}
    }
}