using System.Threading.Tasks;

namespace Bolt.Common
{
    internal static class CompletedTask
    {
        public static readonly Task<bool> True = Task.FromResult(true);

        public static readonly Task<bool> False = Task.FromResult(false);

        public static readonly Task Done = True;
    }
}