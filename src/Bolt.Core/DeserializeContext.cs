using System;
using System.Collections.Generic;
using System.IO;

namespace Bolt
{
    public class DeserializeContext
    {
        public Stream Stream { get; set; }

        public IList<ParameterMetadata> ExpectedValues { get; set; }

        public IList<KeyValuePair<string, object>> Values { get; set; }
    }
}