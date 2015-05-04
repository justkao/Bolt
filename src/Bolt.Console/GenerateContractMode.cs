using System;

namespace Bolt.Console
{
    [Flags]
    public enum GenerateContractMode
    {
        Descriptor = 1,
        Server = 2,
        Client = 4,
        All = Descriptor | Server | Client,
        ClientFull = Descriptor | Client,
        ServerFull = Descriptor | Server
    }
}
