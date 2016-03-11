using System;
using System.IO;

namespace Bolt.Serialization
{
    public class SerializeContext
    {
        public SerializeContext(Stream stream, ActionContextBase actionContext)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }


            Stream = stream;
            ActionContext = actionContext;
        }

        public Stream Stream { get;  }

        public ActionContextBase ActionContext { get; }
    }
}