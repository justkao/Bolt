using System.Threading.Tasks;

namespace Bolt.Server
{
    internal static class CompletedTask
    {
        public static readonly Task Done = Task.FromResult(true);
    }
}