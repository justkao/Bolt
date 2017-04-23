using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt.Serialization
{
    public class ReadParametersContext : SerializeContext
    {
        public ReadParametersContext(Stream stream, ActionContextBase actionContext, IReadOnlyList<ParameterMetadata> parameters) : base(stream, actionContext)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public IReadOnlyList<ParameterMetadata> Parameters { get; }

        public IList<ParameterValue> ParameterValues { get; set; }
    }
}