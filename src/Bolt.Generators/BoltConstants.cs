namespace Bolt.Generators
{
    internal static class BoltConstants
    {
        internal static class Core
        {
            public const string Namespace = "Bolt";

            public static readonly ClassDescriptor AsyncOperationAttribute = new ClassDescriptor("AsyncOperationAttribute", Namespace);

            public static readonly ClassDescriptor InitSessionAttribute = new ClassDescriptor("InitSessionAttribute", Namespace);

            public static readonly ClassDescriptor CloseSessionAttribute = new ClassDescriptor("CloseSessionAttribute", Namespace);

            public static readonly ClassDescriptor Empty = new ClassDescriptor("Empty", Namespace);
        }

        internal static class Client
        {
            public const string Namespace = "Bolt.Client";

            public static readonly ClassDescriptor Channel = new ClassDescriptor("IChannel", Namespace) { IsInterface = true };

            public static readonly ClassDescriptor ProxyBase = new ClassDescriptor("ProxyBase", Namespace) { IsInterface = false };
        }
    }
}
