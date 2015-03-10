namespace Bolt.Generators
{
    internal static class BoltConstants
    {
        public const string ActionDescriptor = "Bolt.ActionDescriptor";

        public const string ContractDescriptor = "Bolt.ContractDescriptor";

        public const string AsyncOperationAttribute = "Bolt.AsyncOperationAttribute";

        public const string InitSessionAttribute = "Bolt.InitSessionAttribute";

        public const string CloseSessionAttribute = "Bolt.CloseSessionAttribute";

        public const string EmptyNamespace = "Bolt";

        public const string EmptyName = "Empty";

        public const string Empty = EmptyNamespace + "."  + EmptyName;

        public const string InvokerName = "ContractInvoker";

        public const string ServerExecutionContext = "Bolt.Server.ServerActionContext";

        public const string BoltServerNamespace = "Bolt.Server";

        public const string BoltServerOptions = "Bolt.Server.BoltServerOptions";

        public const string BoltRouteHandler = "Bolt.Server.IBoltRouteHandler";

        public const string BoltClientNamespace = "Bolt.Client";

        public const string BoltChannelsNamespace = "Bolt.Client.Channels";

        public const string BoltChannelInterface = "IChannel";
    }
}
