namespace Bolt.Generators
{
    internal static class BoltConstants
    {
        internal static class Core
        {
            public const string Namespace = "Bolt";

            public const string PipelineNamespace = "Bolt.Pipeline";

            public static readonly ClassDescriptor AsyncOperationAttribute = new ClassDescriptor("AsyncOperationAttribute", Namespace);

            public static readonly ClassDescriptor InitSessionAttribute = new ClassDescriptor("InitSessionAttribute", Namespace);

            public static readonly ClassDescriptor CloseSessionAttribute = new ClassDescriptor("CloseSessionAttribute", Namespace);

            public static readonly ClassDescriptor Empty = new ClassDescriptor("Empty", Namespace);
        }

        internal static class Client
        {
            public const string Namespace = "Bolt.Client";

            public const string PipelineNamespace = "Bolt.Client.Pipeline";

            public static readonly ClassDescriptor Pipeline = new ClassDescriptor("IClientPipeline", PipelineNamespace) { IsInterface = true };

            public static readonly ClassDescriptor ProxyBase = new ClassDescriptor("ProxyBase", Namespace) { IsInterface = false };
        }
    }
}
