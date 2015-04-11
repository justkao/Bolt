namespace Bolt.Generators
{
    internal static class BoltConstants
    {
        internal static class Core
        {
            public const string Namespace = "Bolt";

            public static readonly ClassDescriptor ActionDescriptor = new ClassDescriptor("ActionDescriptor", Namespace);

            public static readonly ClassDescriptor ContractDescriptor = new ClassDescriptor("ContractDescriptor", Namespace);

            public static readonly ClassDescriptor AsyncOperationAttribute = new ClassDescriptor("AsyncOperationAttribute", Namespace);

            public static readonly ClassDescriptor InitSessionAttribute = new ClassDescriptor("InitSessionAttribute", Namespace);

            public static readonly ClassDescriptor CloseSessionAttribute = new ClassDescriptor("CloseSessionAttribute", Namespace);

            public static readonly ClassDescriptor Empty = new ClassDescriptor("Empty", Namespace);
        }

        internal static class Server
        {
            public const string Namespace = "Bolt.Server";

            public const string InstanceProvidersNamespace = "Bolt.Server.InstanceProviders";

            public static readonly ClassDescriptor ContractActions = new ClassDescriptor("ContractActions", Namespace);

            public static readonly ClassDescriptor ServerActionContext = new ClassDescriptor("ServerActionContext", Namespace);

            public static readonly ClassDescriptor BoltServerOptions = new ClassDescriptor("BoltServerOptions", Namespace);

            public static readonly ClassDescriptor BoltRouteHandlerInterface = new ClassDescriptor("IBoltRouteHandler", Namespace) { IsInterface = true };
        }

        internal static class Client
        {
            public const string Namespace = "Bolt.Client";

            public const string ChannelsNamespace = Namespace + ".Channels";

            public static readonly ClassDescriptor Channel = new ClassDescriptor("IChannel", Namespace) { IsInterface = true };
        }
    }
}
