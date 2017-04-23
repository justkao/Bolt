using System;
using System.IO;

namespace Bolt.Serialization
{
    public class SerializeContext
    {
        public SerializeContext(Stream stream, ActionContextBase actionContext)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            ActionContext = actionContext ?? throw new ArgumentNullException(nameof(actionContext));
        }

        public Stream Stream { get;  }

        public ActionContextBase ActionContext { get; }
    }
}