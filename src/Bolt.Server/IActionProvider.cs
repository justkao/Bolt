using Microsoft.Owin;

namespace Bolt.Server
{
    public interface IActionProvider
    {
        ActionDescriptor GetAction(IOwinContext context);
    }
}
