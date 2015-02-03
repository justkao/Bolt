using System;

namespace Bolt
{
    /// <summary>
    /// Base class for server and client action context.
    /// </summary>
    public abstract class ActionContextBase
    {
        protected ActionContextBase(ActionDescriptor action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            Action = action;
        }

        public ActionDescriptor Action { get; private set; }
    }
}
