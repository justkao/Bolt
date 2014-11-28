using System;

namespace Bolt
{
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
