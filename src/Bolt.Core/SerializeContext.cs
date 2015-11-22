using System.Collections.Generic;
using System.IO;

namespace Bolt
{
    public class SerializeContext
    {
        public Stream Stream { get; set; }

        public IList<KeyValuePair<string, object>> Values { get; set; }
    }
}