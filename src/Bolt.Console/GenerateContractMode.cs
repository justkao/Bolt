using System;

namespace Bolt.Console
{
    [Flags]
    public enum GenerateContractMode
    {
        Proxy = 1,
        Interface = 2,
        All = Proxy | Interface
    }
}
