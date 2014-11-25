using System;

namespace Bolt
{
    public abstract class ExecutionContextBase
    {
        protected ExecutionContextBase(ActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            ActionDescriptor = actionDescriptor;
        }

        public ActionDescriptor ActionDescriptor { get; private set; }
    }
}
