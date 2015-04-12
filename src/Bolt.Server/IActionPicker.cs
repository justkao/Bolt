namespace Bolt.Server
{
    public interface IActionPicker
    {
        ActionDescriptor PickAction(ServerActionContext context, string actionName);
    }
}