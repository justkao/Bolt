using System.Collections.Generic;
using System.IO;

namespace Bolt
{
    public class DeserializeContext
    {
        public Stream Stream { get; set; }

        public IReadOnlyList<ParameterMetadata> Parameters { get; set; }

        public IList<ParameterValue> ParameterValues { get; set; }
    }
}