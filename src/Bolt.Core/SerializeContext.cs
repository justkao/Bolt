using System.Collections.Generic;
using System.IO;

namespace Bolt
{
    public class SerializeContext
    {
        public Stream Stream { get; set; }

        public IList<ParameterValue> ParameterValues { get; set; }
    }
}