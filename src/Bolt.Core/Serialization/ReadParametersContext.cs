using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Serialization
{
    public class ReadParametersContext : SerializeContext
    {
        public ReadParametersContext(Stream stream, ActionContextBase actionContext, IReadOnlyList<ParameterMetadata> parameters) : base(stream, actionContext)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Parameters = parameters;
        }

        public IReadOnlyList<ParameterMetadata> Parameters { get; }

        public IList<ParameterValue> ParameterValues { get; set; }
    }
}