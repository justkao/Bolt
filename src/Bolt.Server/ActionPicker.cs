namespace Bolt.Server
{
    public class ActionPicker : IActionPicker
    {
        public ActionDescriptor PickAction(ServerActionContext context, string actionName)
        {
            return context.ContractInvoker.Descriptor.Find(actionName);
        }
    }
}