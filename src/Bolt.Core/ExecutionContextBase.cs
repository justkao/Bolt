namespace Bolt
{
    public abstract class ExecutionContextBase
    {
        protected ExecutionContextBase(ActionDescriptor actionDescriptor)
        {
            ActionDescriptor = actionDescriptor;
        }

        public ActionDescriptor ActionDescriptor { get; private set; }
    }
}
