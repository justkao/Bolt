using System;

namespace Bolt.Serialization
{
    public class SerializeExceptionContext
    {
        public SerializeExceptionContext(ActionContextBase actionContext)
        {
            ActionContext = actionContext;
        }

        public ActionContextBase ActionContext { get; }
    }
}